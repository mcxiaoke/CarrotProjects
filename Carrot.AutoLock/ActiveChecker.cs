using Carrot.Common;
using Microsoft.Win32;
using System;
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
    public const string DEFAULT_IP = "192.168.1.100";

    /// <summary>
    /// Max offline count threshold before locking.
    /// 设备离线检测次数阈值，连续离线多少次后触发锁定。
    /// </summary>
    public const int MAX_OFFLINE_COUNT = 6;

    /// <summary>
    /// Inactive duration threshold in seconds before assuming absence.
    /// 键盘鼠标无操作时间阈值 (秒)，超过此时间才认为用户可能离开。
    /// </summary>
    public const int INACTIVE_SECONDS = 60;

    // 跨线程使用的布尔值，增加 volatile 保证线程间读取最新值
    private volatile bool _isScreenLocked;
    private volatile bool _checkerRunning;
    private volatile bool _deviceOnline;

    private int _offlineCount;
    private string _targetIP = DEFAULT_IP;

    private CancellationTokenSource? _cancellationTokenSource;

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

    public void SetTargetIP(string targetIP) {
        _targetIP = targetIP;
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
    public double GetInactiveSeconds() {
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

    private bool ShouldCheckStatus() {
        return GetInactiveSeconds() > INACTIVE_SECONDS;
    }

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

        Task.Run(() => CheckDeviceStatusLoop(_cancellationTokenSource.Token));

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
        Callback?.Invoke("");
    }

    private async Task CheckDeviceStatusLoop(CancellationToken cancellationToken) {
        while (_checkerRunning && !cancellationToken.IsCancellationRequested) {
            try {
                // Skip check if screen is locked
                if (_isScreenLocked) {
                    Logger.Info("Screen locked, skip check");
                    await Task.Delay(3000, cancellationToken);
                    continue;
                }

                // Check device status
                bool isOnline = await CheckDeviceStatusAsync();
                bool statusChanged = isOnline != _deviceOnline;
                _deviceOnline = isOnline;

                Logger.Info($"Device: {_targetIP}, Online: {isOnline}, OffCount: {_offlineCount}/{MAX_OFFLINE_COUNT}, " +
                    $"Inactive: {GetInactiveSeconds():F1}s/{INACTIVE_SECONDS}s, ShouldCheck: {ShouldCheckStatus()}");

                if (isOnline) {
                    _offlineCount = 0;
                } else {
                    _offlineCount++;
                    if (ShouldCheckStatus()) {
                        if (_offlineCount >= MAX_OFFLINE_COUNT) {
                            LockWorkStation();
                        }
                    }
                }

                // Only trigger callback if status changed to reduce unnecessary UI updates.
                if (statusChanged) {
                    Callback?.Invoke("");
                }
                await Task.Delay(3000, cancellationToken);
            } catch (OperationCanceledException) {
                Logger.Info("CheckDeviceStatusLoop cancelled");
                break;
            } catch (Exception ex) {
                Logger.Error("Error in CheckDeviceStatusLoop", ex);
                // 发生其它异常时休眠一下防止死循环狂飙，同时也支持 Cancellation
                try {
                    await Task.Delay(3000, cancellationToken);
                } catch (OperationCanceledException) {
                    break;
                }
            }
        }
    }


    private async Task<bool> CheckDeviceStatusAsync() {
        try {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(_targetIP, 1000);
            Logger.Debug($"Ping result: {reply.Status} {_targetIP}");
            if (reply.Status == IPStatus.Success) {
                return true;
            }
        } catch (PingException e) {
            Logger.Debug($"Ping error: {e.Message} {_targetIP}");
        }

        // Fallback to ARP table check using new ArpHelper
        // 如果 Ping 失败，检查 ARP 表中是否有该设备。
        // Originally NetUtils.GetOnlineDevices(), now ArpHelper.GetOnlineDevices()
        var onlineDevices = ArpHelper.GetOnlineDevices();
        // We use string match. ArpHelper.GetOnlineDevices returns List<string>.
        return onlineDevices.Contains(_targetIP);
    }

    /// <summary>
    /// Locks the workstation.
    /// 锁定工作站 (Win + L)。
    /// </summary>
    private void LockWorkStation() {
        Logger.Info($"{_targetIP} lock screen now");
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
            _offlineCount = 0;
            // 移除 _lastActive 重置，GetLastInputInfo 是取系统原生空闲时间，自动跟随系统刷新
        } else if (e.Reason == SessionSwitchReason.SessionLock) {
            Logger.Info("SessionLock: stop timer");
            _isScreenLocked = true;
            _offlineCount = 0;
        }
        Callback?.Invoke("");
    }

    /// <summary>
    /// 实现 IDisposable 清理资源
    /// </summary>
    public void Dispose() {
        Stop();
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}