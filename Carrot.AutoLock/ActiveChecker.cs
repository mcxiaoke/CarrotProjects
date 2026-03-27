using Carrot.AutoLock.Router;
using Carrot.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Carrot.AutoLock;

/// <summary>
/// Active status checker.
/// Monitors device online status and user activity to lock workstation or adjust brightness.
/// 活跃状态检测器。监控设备在线状态及用户活动，自动锁定或调整亮度。
/// </summary>
public class ActiveChecker : IDisposable {

    /// <summary>
    /// Callback delegate for status changes.
    /// 状态变更回调委托。
    /// </summary>
    public delegate void StatusCallback(string result);

    /// <summary>
    /// Default target IP.
    /// 默认要监视的设备的 IP 地址。
    /// </summary>
    public const string DEFAULT_IP = AppConfig.DefaultTargetIP;

    /// <summary>
    /// Offline duration threshold in seconds before locking.
    /// 设备离线时间阈值 (秒)，连续离线超过此时间后触发锁定。
    /// </summary>
    public const int OFFLINE_THRESHOLD = 300;

    /// <summary>
    /// Inactive duration threshold in seconds before assuming absence.
    /// 键盘鼠标无操作时间阈值 (秒)，超过此时间才认为用户可能离开。
    /// </summary>
    public const int INACTIVE_THRESHOLD = 240;

    /// <summary>
    /// 循环检测间隔时间，毫秒
    /// </summary>
    public const int LOOP_DELAY_MS = 8000;

    // 可配置的超时时间
    private int _offlineThreshold = OFFLINE_THRESHOLD;
    private int _inactiveThreshold = INACTIVE_THRESHOLD;

    // 跨线程使用的布尔值，增加 volatile 保证线程间读取最新值
    private volatile bool _isScreenLocked;
    private volatile bool _checkerRunning;
    private volatile bool _deviceOnline;

    // 离线开始时间 (用于计算离线时长)
    private DateTime? _offlineStartTime;
    private string _targetIP = DEFAULT_IP;

    private CancellationTokenSource? _cancellationTokenSource;

    // 上次记录状态信息的时间（用于每3分钟记录一次）
    private DateTime _lastStatusLogTime = DateTime.MinValue;

    // 实例化蓝牙检测器
    private readonly BluetoothDetector _bluetoothDetector = new();

    // 目标手机的蓝牙名称，可以开放给 UI 让用户配置
    //  Paired: 'ZXK M14', ID=Bluetooth#Bluetoothb0:a4:60:6a:e9:af-20:3b:34:54:8a:1d
    private string _targetBluetoothName = "ZXK M14";

    // 目标设备 MAC 地址 (用于路由器检测)
    private string _targetMac = AppConfig.DefaultTargetMac;

    // 路由器检测器
    private TPLinkRouter? _router;

    // 通知管理器
    private readonly NotificationManager _notificationManager = new();

    // 豁免锁屏的进程名称列表
    private List<string> _exemptProcesses = new();

    // 上次检测到豁免进程的时间（用于日志去重）
    private DateTime _lastExemptProcessLogTime = DateTime.MinValue;

    /// <summary>
    /// Status update callback.
    /// 状态回调函数。
    /// </summary>
    public StatusCallback? Callback { get; set; }

    public ActiveChecker() {
    }

    /// <summary>
    /// Gets whether the checker is running.
    /// 获取检测器是否正在运行。
    /// </summary>
    public bool IsRunning() => _checkerRunning;

    /// <summary>
    /// Gets whether the target device is online.
    /// 获取目标设备是否在线。
    /// </summary>
    public bool IsDeviceOnline() => _deviceOnline;

    /// <summary>
    /// Gets the offline duration in seconds.
    /// 获取设备离线时长 (秒)。
    /// </summary>
    public double OfflineSeconds => _offlineStartTime.HasValue
        ? (DateTime.Now - _offlineStartTime.Value).TotalSeconds
        : 0;

    public void SetTargetIP(string targetIP) {
        _targetIP = targetIP;
    }

    /// <summary>
    /// 供外部(如UI)动态修改要检测的蓝牙名称
    /// </summary>
    public void SetTargetBluetoothName(string bluetoothName) {
        _targetBluetoothName = bluetoothName;
    }

    /// <summary>
    /// 设置目标设备 MAC 地址
    /// </summary>
    public void SetTargetMac(string mac) {
        _targetMac = mac;
    }

    /// <summary>
    /// 设置路由器密码以启用路由器检测
    /// </summary>
    public void SetRouterPassword(string password) {
        if (!string.IsNullOrEmpty(password)) {
            _router = new TPLinkRouter(password);
        }
    }

    /// <summary>
    /// 设置超时时间
    /// </summary>
    public void SetTimeoutSecs(int offlineThreshold, int inactiveThreshold) {
        _offlineThreshold = offlineThreshold;
        _inactiveThreshold = inactiveThreshold;
    }

    /// <summary>
    /// 获取通知管理器，用于配置通知渠道
    /// Get notification manager for configuring notification channels
    /// </summary>
    public NotificationManager GetNotificationManager() {
        return _notificationManager;
    }

    /// <summary>
    /// 设置豁免锁屏的进程列表
    /// Set the list of processes that prevent screen lock
    /// </summary>
    /// <param name="processes">进程名称列表（不含 .exe 后缀）</param>
    public void SetExemptProcesses(List<string> processes) {
        _exemptProcesses = processes ?? new List<string>();
        if (_exemptProcesses.Count > 0) {
            Logger.Info($"Exempt processes configured: {string.Join(", ", _exemptProcesses)}");
        }
    }

    #region Windows API - GetLastInputInfo (替代全局键鼠 Hook)

    internal struct LASTINPUTINFO {
        public uint cbSize;
        public uint dwTime;
    }
    [DllImport("User32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    /// <summary>
    /// 通过系统 API 获取键鼠空闲时间，彻底免除第三方 Hook
    /// </summary>
    public static double GetInactiveSeconds() {
        var lastInputInfo = new LASTINPUTINFO();
        lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);

        if (GetLastInputInfo(ref lastInputInfo)) {
            // 使用 Environment.TickCount64 避免 24 天溢出问题
            long systemUptime = Environment.TickCount64;
            long lastInputTicks = lastInputInfo.dwTime;
            return (systemUptime - lastInputTicks) / 1000.0;
        }
        return 0;
    }

    #endregion

    /// <summary>
    /// Starts the monitoring service.
    /// 启动检测服务。
    /// </summary>
    public void Start() {
        if (_checkerRunning) return; // 防止重复启动

        Logger.Info("Start");

        _checkerRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        // 开启蓝牙雷达扫描模式
        //_bluetoothDetector.StartBleScanner();

        Task.Run(() => CheckLoop(_cancellationTokenSource.Token));

        Callback?.Invoke("");
    }

    /// <summary>
    /// Stops the monitoring service.
    /// 停止检测服务。
    /// </summary>
    public void Stop() {
        if (!_checkerRunning) return; // 防止重复停止

        Logger.Info("Stop");
        _checkerRunning = false;
        _cancellationTokenSource?.Cancel();
        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

        // 停止蓝牙扫描
        //_bluetoothDetector.StopBleScanner();

        Callback?.Invoke("");
    }

    private async Task CheckLoop(CancellationToken cancellationToken) {
        while (_checkerRunning && !cancellationToken.IsCancellationRequested) {

            try {
                // Skip check if screen is locked
                if (_isScreenLocked) {
                    Logger.Debug("Screen locked, skip check");
                    await Task.Delay(LOOP_DELAY_MS * 2, cancellationToken);
                    continue;
                }

                // Check device status (Wi-Fi + Bluetooth fallback)
                bool isOnline = await CheckDeviceAsync();
                bool statusChanged = isOnline != _deviceOnline;
                _deviceOnline = isOnline;

                if (isOnline) {
                    _offlineStartTime = null;
                } else {
                    // 记录离线开始时间
                    _offlineStartTime ??= DateTime.Now;
                }

                var offlineSeconds = OfflineSeconds;
                var inactiveSeconds = GetInactiveSeconds();

                // 只在离线时间达到阈值时才检测豁免进程
                bool hasExemptProcess = false;
                if (offlineSeconds >= _offlineThreshold && _exemptProcesses.Count > 0) {
                    hasExemptProcess = ProcessChecker.IsAnyProcessRunning(_exemptProcesses);
                    if (hasExemptProcess) {
                        if ((DateTime.Now - _lastExemptProcessLogTime).TotalMinutes >= 3) {
                            Logger.ConsoleInfo("Exempt process running, skipping lock check");
                            _lastExemptProcessLogTime = DateTime.Now;
                        }
                    }
                }

                if (!hasExemptProcess) {
                    if (inactiveSeconds > _inactiveThreshold
                        && offlineSeconds >= _offlineThreshold) {
                        // 条件2：设备离线 + 用户无操作
                        Logger.Warning($"Device offline {offlineSeconds:F0}s and " +
                            $"user inactive {inactiveSeconds:F1}s, " +
                            $"locking workstation...");
                        LockWorkStation();
                    }
                }

                var statusInfo = $"Device: {_targetIP} | {_targetBluetoothName}\r\n" +
                        $"Offline: {offlineSeconds:F0}s/{_offlineThreshold}s, " +
                        $"Inactive: {inactiveSeconds:F1}s/{_inactiveThreshold}s";

                // 每 3 分钟记录一次状态信息到日志
                var now = DateTime.Now;
                if ((now - _lastStatusLogTime).TotalMinutes >= 3) {
                    Logger.ConsoleInfo(statusInfo);
                    _lastStatusLogTime = now;
                } else {
                    Logger.Debug(statusInfo);
                }

                Callback?.Invoke("");
                await Task.Delay(LOOP_DELAY_MS, cancellationToken);
            } catch (OperationCanceledException) {
                Logger.Info("cancelled");
                break;
            } catch (Exception ex) {
                Logger.Error("Error", ex);
                // 发生其它异常时休眠一下防止死循环狂飙，同时也支持 Cancellation
                try {
                    await Task.Delay(LOOP_DELAY_MS, cancellationToken);
                } catch (OperationCanceledException) {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 核心检测逻辑：路由器 -> Wi-Fi -> 蓝牙多重检测
    /// </summary>
    private async Task<bool> CheckDeviceAsync() {


        // 第一层检测：Wi-Fi 网络 (Ping & ARP)
        bool isWifiOnline = await CheckWifiAsync();
        if (isWifiOnline) {
            Logger.Debug($"Ping result: OK {_targetIP}");
            return true;
        }

        // 第二层检测：路由器在线设备检测 (通过 MAC 地址)
        if (_router != null && !string.IsNullOrEmpty(_targetMac)) {
            try {
                bool isRouterOnline = await _router.IsOnlineAsync(_targetMac);
                if (isRouterOnline) {
                    Logger.Debug($"Router detected [{_targetMac}] online.");
                    return true;
                }
            } catch (Exception ex) {
                Logger.Debug($"Router check failed: {ex.Message}");
            }
        }

        // Wi-Fi 离线，进入蓝牙后备检测
        if (!string.IsNullOrEmpty(_targetBluetoothName)) {
            Logger.Debug($"Wi-Fi [{_targetIP}] offline. Bluetooth check for [{_targetBluetoothName}]...");

            // 第三层检测：查询系统蓝牙配对状态
            bool isBluetoothConnected = await _bluetoothDetector.IsPairedDeviceConnectedAsync(_targetBluetoothName);
            if (isBluetoothConnected) {
                Logger.Debug($"[{_targetBluetoothName}] is paired and connected via Bluetooth.");
                return true;
            }
        }

        // 所有手段都检测不到，判定为离线
        return false;
    }

    /// <summary>
    /// 原先的 Wi-Fi 状态检测逻辑 (Ping + ARP)
    /// </summary>
    private async Task<bool> CheckWifiAsync() {
        // 第一层：Ping 目标 IP（最快）
        try {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(_targetIP, 1000);
            //Logger.Debug($"Ping result: {reply.Status} {_targetIP}");
            if (reply.Status == IPStatus.Success) {
                return true;
            }
        } catch (PingException e) {
            Logger.Debug($"Ping error: {e.Message} {_targetIP}");
        }

        // 第二层：ARP 缓存匹配
        var onlineDevices = ArpHelper.GetOnlineDevices();
        return onlineDevices.Contains(_targetIP);
    }

    /// <summary>
    /// Locks the workstation.
    /// 锁定工作站 (Win + L)。
    /// </summary>
    private void LockWorkStation() {
        Logger.Info($"{_targetIP} / {_targetBluetoothName} lock screen now");

        // 发送通知（异步，不阻塞锁定流程）
        try {
            var deviceInfo = string.IsNullOrEmpty(_targetBluetoothName) ? _targetIP : $"{_targetIP} / {_targetBluetoothName}";
            var reason = $"设备离线 {OfflineSeconds:F0}秒，用户无活动 {GetInactiveSeconds():F0}秒";
            _notificationManager.SendLockNotification(deviceInfo, reason);
        } catch (Exception ex) {
            Logger.Error("Failed to send lock notification", ex);
        }

        // 执行锁定
        try {
            bool result = LockWorkStationInternal();
            if (!result) {
                int errorCode = Marshal.GetLastWin32Error();
                Logger.Warning($"Failed to lock workstation. Error code: {errorCode}");
            }
            _isScreenLocked = true;
        } catch (Exception ex) {
            Logger.Error("LockWorkStation", ex);
        }
    }
    [DllImport("user32.dll", EntryPoint = "LockWorkStation", SetLastError = true)]
    private static extern bool LockWorkStationInternal();

    private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
        if (e.Reason == SessionSwitchReason.SessionUnlock) {
            Logger.Info("SessionUnlock: reset timer");
            _isScreenLocked = false;
            _offlineStartTime = null;
        } else if (e.Reason == SessionSwitchReason.SessionLock) {
            Logger.Info("SessionLock: stop timer");
            _isScreenLocked = true;
            _offlineStartTime = null;
        }
        Callback?.Invoke("");
    }

    /// <summary>
    /// 实现 IDisposable 清理资源
    /// </summary>
    public void Dispose() {
        Stop();
        _cancellationTokenSource?.Dispose();
        _bluetoothDetector.Dispose(); // 确保蓝牙雷达资源也被释放
        _notificationManager.Dispose(); // 释放通知管理器资源
        GC.SuppressFinalize(this);
    }
}
