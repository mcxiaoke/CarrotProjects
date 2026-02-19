using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;
using Gma.System.MouseKeyHook;
using System.Diagnostics;
using Carrot.Common;
using Gma;

namespace Carrot.AutoLock {

    /// <summary>
    /// 活跃状态检测器
    /// 负责监控指定设备的在线状态，并在满足条件时自动锁定工作站
    /// 同时根据时间和锁屏状态调整显示器亮度
    /// </summary>
    public class ActiveChecker {

        /// <summary>
        /// 状态变更回调委托
        /// </summary>
        /// <param name="result">状态描述</param>
        public delegate void StatusCallback(string result);

        /// <summary>
        /// 默认要监视的设备的 IP 地址
        /// </summary>
        public static readonly string DEFAULT_IP = "192.168.1.40";

        /// <summary>
        /// 设备离线检测次数阈值，连续离线多少次后触发锁定
        /// </summary>
        public static readonly int MAX_OFFLINE_COUNT = 6;

        /// <summary>
        /// 键盘鼠标无操作时间阈值 (秒)，超过此时间才认为用户可能离开
        /// </summary>
        public static readonly int INACTIVE_SECONDS = 60;


        bool isScreenLocked = false;   // 屏幕是否已锁定
        bool checkerRunning = false;   // 检测器是否正在运行
        bool deviceOnline = false;     // 目标设备是否在线
        int offlineCount = 0;          // 连续离线计数

        private string targetIP = DEFAULT_IP;

        private DateTime lastActive;   // 最后一次键鼠活动时间
        private IKeyboardMouseEvents? m_GlobalHook; // 全局键鼠钩子

        /// <summary>
        /// 状态回调函数
        /// </summary>
        public StatusCallback? callback;

        /// <summary>
        /// 显示器亮度管理器
        /// </summary>
        private MonitorManager? monitorManager;

        public ActiveChecker() {
            this.lastActive = DateTime.Now;
        }

        /// <summary>
        /// 获取检测器是否正在运行
        /// </summary>
        public bool IsRunning() {
            return checkerRunning;
        }

        public bool IsDeviceOnline() {
            return deviceOnline;
        }

        public void SetTargetIP(string targetIP) {
            this.targetIP = targetIP;
        }

        public double GetInactiveSeconds() {
            return (DateTime.Now - lastActive).TotalSeconds;
        }

        bool ShouldCheckStatus() {
            return GetInactiveSeconds() > INACTIVE_SECONDS;
        }

        /// <summary>
        /// 启动检测服务
        /// 初始化监视器、全局钩子，并开始后台检测任务
        /// </summary>
        public void Start() {
            Logger.Info("Start");
            lastActive = DateTime.Now;
            monitorManager = new MonitorManager();
            try {
                monitorManager.Initialize();
                // 立即应用初始亮度
                if (!isScreenLocked) {
                    monitorManager.SetAllBrightness(GetTargetBrightness());
                }
            } catch (Exception ex) {
                Logger.Error("MonitorManager init failed", ex);
            }

            checkerRunning = true;
            // 订阅系统会话切换事件 (锁屏/解锁)
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            Task.Run(async () => {
                await CheckDeviceStatusLoop();
            });
            Subscribe(); // 注册全局钩子
            callback?.Invoke("");
        }


        /// <summary>
        /// 停止检测服务
        /// 释放资源、取消订阅
        /// </summary>
        public void Stop() {
            Logger.Info("Stop");
            Unsubscribe();
            checkerRunning = false;
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
            monitorManager?.Dispose();
            monitorManager = null;
            callback?.Invoke("");
        }

        /// <summary>
        /// 后台检测循环任务
        /// 定期检测设备在线状态，并根据条件执行锁定或调整亮度
        /// </summary>
        async Task CheckDeviceStatusLoop() {
            for (; ; ) {
                if (!checkerRunning) {
                    break;
                }
                // 锁屏状态下不进行离线检测，避免重复锁定
                if (isScreenLocked) {
                    Logger.Info($"screen locked, skip check");
                    await Task.Delay(3000);
                    continue;
                }

                // 检测设备在线状态
                bool isOnline = await CheckDeviceStatus();
                this.deviceOnline = isOnline;
                Logger.Info("online:" + isOnline
                    + " offCount:" + offlineCount
                    + " inactive:" + GetInactiveSeconds());

                // 只有当用户长时间未操作 (无键鼠活动) 时，才考虑自动锁定
                if (ShouldCheckStatus()) {
                    if (isOnline) {
                        Logger.Info($"{this.targetIP} online, reset counter");
                        offlineCount = 0; // 设备在线，重置离线计数
                    } else {
                        Logger.Info($"{this.targetIP} offline, increase counter");
                        offlineCount++;
                        // 达到离线阈值，执行锁定
                        if (offlineCount >= MAX_OFFLINE_COUNT) {
                            LockWorkStation();
                        }
                    }
                }

                // 同步亮度
                if (!isScreenLocked) {
                    SyncBrightness();
                }

                callback?.Invoke("");
                // 每 3 秒检测一次
                await Task.Delay(3000);
            }
        }

        /// <summary>
        /// 同步显示器亮度
        /// 根据当前时间段，定期调整屏幕亮度
        /// </summary>
        private void SyncBrightness() {
             // 简单逻辑：每分钟的前几秒尝试同步一次亮度
             // 避免频繁调用 DDC/CI (比较耗时)
             if (DateTime.Now.Second < 5) { // 大约每分钟执行一次
                 uint target = GetTargetBrightness();
                 monitorManager?.SetAllBrightness(target);
             }
        }

        /// <summary>
        /// 根据当前时间获取目标亮度值
        /// </summary>
        /// <returns>亮度值 (0-100)</returns>
        private uint GetTargetBrightness() {
            int hour = DateTime.Now.Hour;
            // 白天 (8点到18点): 80% 亮度
            if (hour >= 8 && hour < 18) {
                return 80;
            }
            // 晚上: 30% 亮度
            return 30;
        }

        /// <summary>
        /// 检查目标设备是否在线
        /// 优先使用 Ping，失败后回退到检查 ARP 表
        /// </summary>
        /// <returns>是否在线</returns>
        async Task<bool> CheckDeviceStatus() {
            try {
                using Ping ping = new();
                PingReply reply = await ping.SendPingAsync(this.targetIP, 1000);
                Logger.Info($"pingok: {reply.Status} {this.targetIP}");
                if (reply.Status == IPStatus.Success) {
                    return true;
                }
            } catch (PingException e) {
                Logger.Info($"pingerr: {e.Message} {this.targetIP}");
            }
            // 如果 Ping 失败，检查 ARP 表中是否有该设备 (通常局域网设备即使不回 Ping 也会在 ARP 表里)
            return NetUtils.GetOnlineDevices().Contains(this.targetIP);
        }

        /// <summary>
        /// 锁定工作站 (Win + L)
        /// </summary>
        void LockWorkStation() {
            Logger.Info($"{this.targetIP} lock screen now");
            isScreenLocked = true;
            Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
        }

        /// <summary>
        /// 处理系统会话切换事件 (锁屏/解锁)
        /// </summary>
        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
            if (e.Reason == SessionSwitchReason.SessionUnlock) {
                // 解锁时：重置计数器，恢复亮度
                Logger.Info("SessionUnlock:reset timer");
                isScreenLocked = false;
                offlineCount = 0;
                lastActive = DateTime.Now;
                monitorManager?.SetAllBrightness(GetTargetBrightness());
            } else if (e.Reason == SessionSwitchReason.SessionLock) {
                // 锁定后：暂停计时，降低亮度 (熄屏)
                Logger.Info("SessionLock:stop timer");
                isScreenLocked = true;
                offlineCount = 0;
                lastActive = DateTime.Now;
                monitorManager?.SetAllBrightness(0); // Dim to 0 (or min supported)
            }
            callback?.Invoke("");
        }


        /// <summary>
        /// 注册全局鼠标键盘钩子，用于检测用户活动
        /// </summary>
        void Subscribe() {
            Logger.Info("Subscribe");
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.MouseMoveExt += GlobalHookMouseMoveExt;
            m_GlobalHook.MouseWheelExt += GlobalHookMouseMoveExt;
            m_GlobalHook.KeyPress += GlobalHookKeyPress;
        }

        private void GlobalHookKeyPress(object sender, KeyPressEventArgs e) {
            Logger.Info("KeyPress: " + e.KeyChar);
            lastActive = DateTime.Now;
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e) {
            Logger.Info("MouseDown: " + e.Button);
            lastActive = DateTime.Now;
            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        private void GlobalHookMouseMoveExt(object sender, MouseEventExtArgs e) {
            //Logger.Info("MouseMove: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);
            lastActive = DateTime.Now;
            // uncommenting the following line will suppress the middle mouse button click
            // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }
        }

        /// <summary>
        /// 取消订阅全局钩子
        /// </summary>
        void Unsubscribe() {
            Logger.Info("Unsubscribe");
            if (m_GlobalHook != null) {
                m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
                m_GlobalHook.MouseMoveExt -= GlobalHookMouseMoveExt;
                m_GlobalHook.MouseWheelExt -= GlobalHookMouseMoveExt;
                m_GlobalHook.KeyPress -= GlobalHookKeyPress;

                //It is recommened to dispose it
                m_GlobalHook.Dispose();
                m_GlobalHook = null;
            }

        }


    }




}
