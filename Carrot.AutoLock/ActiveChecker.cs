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

    public class ActiveChecker {

        public delegate void StatusCallback(string result);

        // 要监视的设备的 IP 地址
        public static readonly string DEFAULT_IP = "192.168.1.106";
        // 设备离线检测次数阈值
        public static readonly int MAX_OFFLINE_COUNT = 6;
        // 键盘鼠标活跃时间阈值
        public static readonly int INACTIVE_SECONDS = 60;


        bool isScreenLocked = false;
        bool checkerRunning = false;
        bool deviceOnline = false;
        int offlineCount = 0;

        private string targetIP = DEFAULT_IP;

        private DateTime lastActive;
        private IKeyboardMouseEvents? m_GlobalHook;

        public StatusCallback? callback;

        public ActiveChecker() {
            this.lastActive = DateTime.Now;
        }


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

        public void Start() {
            Logger.Info("Start");
            lastActive = DateTime.Now;
            checkerRunning = true;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            Task.Run(async () => {
                await CheckDeviceStatusLoop();
            });
            Subscribe();
            callback?.Invoke("");
        }


        public void Stop() {
            Logger.Info("Stop");
            Unsubscribe();
            checkerRunning = false;
            callback?.Invoke("");
        }

        async Task CheckDeviceStatusLoop() {
            for (; ; ) {
                if (!checkerRunning) {
                    break;
                }
                // 锁屏时不检测
                if (isScreenLocked) {
                    Logger.Info($"screen locked, skip check");
                    await Task.Delay(3000);
                    continue;
                }
                //var devices = NetUtils.GetOnlineDevices();
                //foreach (var device in devices) {
                //    Console.WriteLine(device);
                //}
                bool isOnline = await CheckDeviceStatus();
                this.deviceOnline = isOnline;
                Logger.Info("online:" + isOnline
                    + " offCount:" + offlineCount
                    + " inactive:" + GetInactiveSeconds());
                if (ShouldCheckStatus()) {
                    if (isOnline) {
                        Logger.Info($"{this.targetIP} online, reset counter");
                        offlineCount = 0;
                    } else {
                        Logger.Info($"{this.targetIP} offline, increase counter");
                        offlineCount++;
                        if (offlineCount >= MAX_OFFLINE_COUNT) {
                            LockWorkStation();
                        }
                    }
                }
                callback?.Invoke("");
                // 3 秒检测一次
                await Task.Delay(3000);
            }
        }

        async Task<bool> CheckDeviceStatus() {
            try {
                Ping ping = new();
                PingReply reply = await ping.SendPingAsync(this.targetIP, 1000);
                Logger.Info($"pingok: {reply.Status} {this.targetIP}");
                if (reply.Status == IPStatus.Success) {
                    return true;
                }
            } catch (PingException e) {
                Logger.Info($"pingerr: {e.Message} {this.targetIP}");
            }
            return NetUtils.GetOnlineDevices().Contains(this.targetIP);
        }

        void LockWorkStation() {
            Logger.Info($"{this.targetIP} lock screen now");
            isScreenLocked = true;
            Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
        }

        void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
            if (e.Reason == SessionSwitchReason.SessionUnlock) {
                Logger.Info("SessionUnlock:reset timer");
                isScreenLocked = false;
                offlineCount = 0;
                lastActive = DateTime.Now;
            } else if (e.Reason == SessionSwitchReason.SessionLock) {
                Logger.Info("SessionLock:stop timer");
                isScreenLocked = true;
                offlineCount = 0;
                lastActive = DateTime.Now;
            }
            callback?.Invoke("");
        }


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
