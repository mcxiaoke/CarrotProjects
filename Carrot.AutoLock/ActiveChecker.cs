using Carrot.Common;
using Gma.System.MouseKeyHook;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Net.NetworkInformation;

namespace Carrot.AutoLock;

/// <summary>
/// Active status checker.
/// Monitors device online status and user activity to lock workstation or adjust brightness.
/// 活跃状态检测器。监控设备在线状态及用户活动，自动锁定或调整亮度。
/// </summary>
public class ActiveChecker {

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

    private bool _isScreenLocked;
    private bool _checkerRunning;
    private bool _deviceOnline;
    private int _offlineCount;
    private string _targetIP = DEFAULT_IP;
    private DateTime _lastActive = DateTime.Now;

    private IKeyboardMouseEvents? _globalHook;

    /// <summary>
    /// Status update callback.
    /// 状态回调函数。
    /// </summary>
    public StatusCallback? Callback { get; set; }

    public ActiveChecker() {
        _lastActive = DateTime.Now;
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

    public double GetInactiveSeconds() {
        return (DateTime.Now - _lastActive).TotalSeconds;
    }

    private bool ShouldCheckStatus() {
        return GetInactiveSeconds() > INACTIVE_SECONDS;
    }

    /// <summary>
    /// Starts the monitoring service.
    /// 启动检测服务。
    /// </summary>
    public void Start() {
        Logger.Info("Start");
        _lastActive = DateTime.Now;

        _checkerRunning = true;
        SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

        Task.Run(CheckDeviceStatusLoop);

        Subscribe();
        Callback?.Invoke("");
    }

    /// <summary>
    /// Stops the monitoring service.
    /// 停止检测服务。
    /// </summary>
    public void Stop() {
        Logger.Info("Stop");
        Unsubscribe();
        _checkerRunning = false;
        SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        Callback?.Invoke("");
    }

    private async Task CheckDeviceStatusLoop() {
        while (_checkerRunning) {
            // Skip check if screen is locked
            if (_isScreenLocked) {
                Logger.Info("Screen locked, skip check");
                await Task.Delay(3000);
                continue;
            }
            // Check device status
            bool isOnline = await CheckDeviceStatusAsync();
            bool statusChanged = isOnline != _deviceOnline;
            _deviceOnline = isOnline;
            Logger.Info($"Online: {isOnline}, OffCount: {_offlineCount}, " +
                $"Inactive: {GetInactiveSeconds():F1}s");
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
            await Task.Delay(3000);
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
        _isScreenLocked = true;
        LockWorkStationInternal();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "LockWorkStation", SetLastError = true)]
    private static extern bool LockWorkStationInternal();

    private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
        if (e.Reason == SessionSwitchReason.SessionUnlock) {
            Logger.Info("SessionUnlock: reset timer");
            _isScreenLocked = false;
            _offlineCount = 0;
            _lastActive = DateTime.Now;
        } else if (e.Reason == SessionSwitchReason.SessionLock) {
            Logger.Info("SessionLock: stop timer");
            _isScreenLocked = true;
            _offlineCount = 0;
            _lastActive = DateTime.Now;
        }
        Callback?.Invoke("");
    }

    private void Subscribe() {
        Logger.Info("Subscribe");
        // Ensure to dispose previous hook if any?
        Unsubscribe();

        _globalHook = Hook.GlobalEvents();
        _globalHook.MouseDownExt += GlobalHookUserActivity;
        _globalHook.MouseMoveExt += GlobalHookUserActivity;
        _globalHook.MouseWheelExt += GlobalHookUserActivity;
        _globalHook.KeyPress += GlobalHookKeyPress;
    }

    private void Unsubscribe() {
        if (_globalHook != null) {
            Logger.Info("Unsubscribe");
            _globalHook.MouseDownExt -= GlobalHookUserActivity;
            _globalHook.MouseMoveExt -= GlobalHookUserActivity;
            _globalHook.MouseWheelExt -= GlobalHookUserActivity;
            _globalHook.KeyPress -= GlobalHookKeyPress;
            _globalHook.Dispose();
            _globalHook = null;
        }
    }

    private void GlobalHookKeyPress(object sender, KeyPressEventArgs e) {
        // Logger.Info("KeyPress: " + e.KeyChar);
        _lastActive = DateTime.Now;
    }

    private void GlobalHookUserActivity(object sender, MouseEventExtArgs e) {
        // Logger.Info("MouseActivity: " + e.Button);
        _lastActive = DateTime.Now;
    }
}
