using Carrot.Common;
using System;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Carrot.AutoLock;

/// <summary>
/// WebSocket 客户端，用于连接 CarrotBot 消息网关。
/// 支持自动重连、心跳保活、接收远程锁屏命令。
/// </summary>
public class WebSocketClient : IDisposable {
    /// <summary>默认 WebSocket 服务地址</summary>
    private const string DefaultWebSocketUri = "ws://127.0.0.1:3123/ws";
    /// <summary>基础重连延迟（毫秒），实际延迟 = 基础延迟 * 重试次数</summary>
    private const int ReconnectDelayMs = 5000;
    /// <summary>最大重连延迟（毫秒），延迟达到此值后不再增加</summary>
    private const int MaxReconnectDelayMs = 60000;
    /// <summary>接收缓冲区大小（字节）</summary>
    private const int ReceiveBufferSize = 4096;
    /// <summary>默认心跳间隔（毫秒），连接成功后可从服务端获取</summary>
    private const int DefaultHeartbeatIntervalMs = 30000;
    /// <summary>连接超时（毫秒）</summary>
    private const int ConnectTimeoutMs = 10000;

    /// <summary>WebSocket 服务 URI</summary>
    private readonly string _wsUri;
    /// <summary>WebSocket 客户端实例</summary>
    private ClientWebSocket? _webSocket;
    /// <summary>取消令牌源，用于停止连接和接收循环</summary>
    private CancellationTokenSource? _cancellationTokenSource;
    /// <summary>客户端是否正在运行</summary>
    private volatile bool _isRunning;
    /// <summary>是否已连接到服务端</summary>
    private volatile bool _isConnected;
    /// <summary>心跳间隔（毫秒），可从服务端动态获取</summary>
    private int _heartbeatIntervalMs = DefaultHeartbeatIntervalMs;
    /// <summary>重连尝试次数，连接成功后重置为 0</summary>
    private int _reconnectAttempts;

    /// <summary>收到原始消息时触发，参数为 JSON 字符串</summary>
    public event Action<string>? OnMessage;
    /// <summary>连接状态变化时触发，参数为是否已连接</summary>
    public event Action<bool>? OnConnectionChanged;
    /// <summary>收到锁屏命令时触发</summary>
    public event Action? OnLockCommandReceived;

    /// <summary>是否已连接到服务端</summary>
    public bool IsConnected => _isConnected;
    /// <summary>客户端是否正在运行</summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// 创建 WebSocket 客户端实例。
    /// </summary>
    /// <param name="uri">WebSocket 服务地址，默认为 ws://127.0.0.1:3123/ws</param>
    public WebSocketClient(string? uri = null) {
        _wsUri = uri ?? DefaultWebSocketUri;
    }

    /// <summary>
    /// 构建带客户端标识参数的连接 URI。
    /// 添加 user、os、arch、desc 查询参数用于服务端识别客户端。
    /// </summary>
    /// <param name="baseUri">基础 WebSocket URI</param>
    /// <returns>带参数的完整 URI</returns>
    private static string BuildConnectUri(string baseUri) {
        var osPlatform = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows"
            : RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "linux"
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos"
            : "unknown";
        var osArch = RuntimeInformation.OSArchitecture.ToString().ToLower();
        var osDesc = Uri.EscapeDataString(RuntimeInformation.OSDescription);

        var separator = baseUri.Contains('?') ? "&" : "?";
        return $"{baseUri}{separator}user=carrot.autolock&os={osPlatform}&arch={osArch}&desc={osDesc}";
    }

    /// <summary>
    /// 启动 WebSocket 客户端，开始连接和接收消息。
    /// </summary>
    public void Start() {
        if (_isRunning) return;

        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(async () => {
            try {
                await ConnectLoop(_cancellationTokenSource.Token);
            } catch (Exception ex) {
                Logger.Error("ConnectLoop failed", ex);
            }
        });
        Logger.Info($"starting, target: {_wsUri}");
    }

    /// <summary>
    /// 停止 WebSocket 客户端，关闭连接并清理资源。
    /// </summary>
    public void Stop() {
        if (!_isRunning) return;

        Logger.Info("stopping");
        _isRunning = false;
        _cancellationTokenSource?.Cancel();

        try {
            _webSocket?.CloseAsync(WebSocketCloseStatus.NormalClosure, "Stopping", CancellationToken.None).Wait(1000);
        } catch {
            // 忽略关闭错误
        }

        _webSocket?.Dispose();
        _webSocket = null;
        SetConnected(false);
    }

    /// <summary>
    /// 连接循环，负责建立连接、断开后自动重连。
    /// 重连延迟采用指数退避策略，最大不超过 MaxReconnectDelayMs。
    /// </summary>
    private async Task ConnectLoop(CancellationToken cancellationToken) {
        Logger.Info($"ConnectLoop started, _isRunning={_isRunning}, cancelled={cancellationToken.IsCancellationRequested}");
        while (_isRunning && !cancellationToken.IsCancellationRequested) {
            try {
                var connectUri = BuildConnectUri(_wsUri);
                Logger.Info($"connecting to {connectUri}");
                _webSocket = new ClientWebSocket();

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(ConnectTimeoutMs);

                await _webSocket.ConnectAsync(new Uri(connectUri), cts.Token);

                Logger.Info("connected");
                SetConnected(true);
                _reconnectAttempts = 0;

                var heartbeatTask = HeartbeatLoop(cancellationToken);
                var receiveTask = ReceiveLoop(cancellationToken);

                await Task.WhenAny(heartbeatTask, receiveTask);
            } catch (OperationCanceledException) {
                if (cancellationToken.IsCancellationRequested) {
                    break;
                }
                Logger.Warning($"connection timeout after {ConnectTimeoutMs}ms");
            } catch (WebSocketException ex) {
                Logger.Warning($"connection error: {GetWsErrorDesc(ex)}");
            } catch (Exception ex) {
                Logger.ErrorShort($"connection error", ex);
            }

            SetConnected(false);

            if (_webSocket != null) {
                try {
                    _webSocket.Dispose();
                } catch { }
                _webSocket = null;
            }

            if (_isRunning && !cancellationToken.IsCancellationRequested) {
                _reconnectAttempts++;
                var delay = Math.Min(ReconnectDelayMs * _reconnectAttempts, MaxReconnectDelayMs);
                Logger.Warning($"disconnected, reconnecting in {delay}ms (attempt #{_reconnectAttempts})");
                try {
                    await Task.Delay(delay, cancellationToken);
                } catch (OperationCanceledException) {
                    break;
                }
            }
        }

        SetConnected(false);
        Logger.Info("stopped");
    }

    /// <summary>
    /// 心跳循环，定期发送 ping 消息保持连接活跃。
    /// </summary>
    private async Task HeartbeatLoop(CancellationToken cancellationToken) {
        while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested) {
            try {
                await Task.Delay(_heartbeatIntervalMs, cancellationToken);

                if (_webSocket?.State == WebSocketState.Open) {
                    var pingMessage = Encoding.UTF8.GetBytes("{\"type\":\"ping\"}");
                    await _webSocket.SendAsync(new ArraySegment<byte>(pingMessage), WebSocketMessageType.Text, true, cancellationToken);
                    Logger.Debug("sent heartbeat ping");
                }
            } catch (OperationCanceledException) {
                break;
            } catch (WebSocketException ex) {
                Logger.Warning($"heartbeat error: {GetWsErrorDesc(ex)}");
                break;
            } catch (Exception ex) {
                Logger.ErrorShort($"heartbeat error", ex);
                break;
            }
        }
    }

    /// <summary>
    /// 消息接收循环，持续接收服务端消息并处理。
    /// </summary>
    private async Task ReceiveLoop(CancellationToken cancellationToken) {
        var buffer = new byte[ReceiveBufferSize];
        var messageBuilder = new StringBuilder();

        while (_webSocket?.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested) {
            try {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close) {
                    Logger.Info("server closed connection");
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text) {
                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                    if (result.EndOfMessage) {
                        var message = messageBuilder.ToString();
                        messageBuilder.Clear();
                        ProcessMessage(message);
                    }
                }
            } catch (OperationCanceledException) {
                break;
            } catch (WebSocketException ex) {
                Logger.Warning($"receive error: {GetWsErrorDesc(ex)}");
                break;
            } catch (Exception ex) {
                Logger.ErrorShort($"receive error", ex);
                break;
            }
        }
    }

    /// <summary>
    /// 处理收到的消息，解析 JSON 并根据类型分发。
    /// 支持的消息类型：connected、pong、message。
    /// </summary>
    /// <param name="message">JSON 格式的消息字符串</param>
    private void ProcessMessage(string message) {
        Logger.Debug($"received: {message}");
        OnMessage?.Invoke(message);

        try {
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (!root.TryGetProperty("type", out var typeProp)) return;

            var type = typeProp.GetString();

            switch (type) {
                case "connected":
                    OnConnectedMsg(root);
                    break;
                case "pong":
                    Logger.Debug("received heartbeat pong");
                    break;
                case "message":
                    OnDataMsg(root);
                    break;
            }
        } catch (JsonException ex) {
            Logger.ErrorShort($"JSON parse error", ex);
        }
    }

    /// <summary>
    /// 处理连接成功消息，提取 clientId 和心跳间隔配置。
    /// </summary>
    private void OnConnectedMsg(JsonElement root) {
        if (root.TryGetProperty("clientId", out var clientIdProp)) {
            Logger.Info($"connected with clientId: {clientIdProp.GetString()}");
        }

        if (root.TryGetProperty("heartbeatInterval", out var intervalProp)) {
            var serverInterval = intervalProp.GetInt32();
            _heartbeatIntervalMs = serverInterval > 0 ? serverInterval : DefaultHeartbeatIntervalMs;
            Logger.Info($"heartbeat interval set to {_heartbeatIntervalMs}ms");
        }
    }

    /// <summary>
    /// 处理数据消息，检查是否为锁屏命令。
    /// 当消息内容为 "/lock mcpc" 时触发锁屏事件。
    /// </summary>
    private void OnDataMsg(JsonElement root) {
        Logger.Info($"received data message: {root.GetRawText()}");
        if (root.TryGetProperty("data", out var dataProp)) {
            if (dataProp.TryGetProperty("content", out var contentProp)) {
                var content = contentProp.GetString();
                if (content == "/lock mcpc") {
                    OnLockCommandReceived?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// 更新连接状态并触发事件。
    /// </summary>
    private void SetConnected(bool connected) {
        if (_isConnected != connected) {
            _isConnected = connected;
            OnConnectionChanged?.Invoke(connected);
        }
    }

    /// <summary>
    /// 获取 WebSocket 异常的简短描述，避免日志过长。
    /// </summary>
    private static string GetWsErrorDesc(WebSocketException ex) {
        return ex.WebSocketErrorCode switch {
            WebSocketError.ConnectionClosedPrematurely => "connection closed by remote",
            WebSocketError.InvalidMessageType => "invalid message type",
            WebSocketError.UnsupportedProtocol => "unsupported protocol",
            WebSocketError.UnsupportedVersion => "unsupported version",
            WebSocketError.NotAWebSocket => "not a websocket",
            WebSocketError.NativeError => $"native error: {ex.NativeErrorCode}",
            _ => ex.Message
        };
    }

    /// <summary>
    /// 释放资源，停止客户端。
    /// </summary>
    public void Dispose() {
        Stop();
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
