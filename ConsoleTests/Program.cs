using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Carrot.Common;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Microsoft.Win32;
using Gma.System.MouseKeyHook;

namespace ConsoleTests {


    public class ARP {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int destIp, int srcIp, byte[] pMacAddr, ref uint phyAddrLen);

        public static PhysicalAddress GetMacAddress(IPAddress ipAddress) {
            byte[] macAddr = new byte[6];
            uint macAddrLen = (uint)macAddr.Length;
            if (SendARP(BitConverter.ToInt32(ipAddress.GetAddressBytes(), 0), 0, macAddr, ref macAddrLen) == 0) {
                return new PhysicalAddress(macAddr);
            }
            return null;
        }
    }

    public static class NetworkScanner {
        public static List<Device> ScanLocalNetwork(string baseIP,
            int startRange = 1, int endRange = 255,
            int concurrency = 50, int timeout = 500) {
            List<Device> devices = new List<Device>();

            // 分割 IP 地址，获取前三个部分
            string[] parts = baseIP.Split('.');
            string subnet = parts[0] + "." + parts[1] + "." + parts[2] + ".";

            // 并发处理
            List<Task> tasks = new List<Task>();
            for (int i = startRange; i <= endRange; i++) {
                string ipAddress = subnet + i;
                tasks.Add(Task.Run(() => {
                    var device = GetDeviceInfo(ipAddress, timeout);
                    if (device != null) {
                        devices.Add(device);
                    }
                }));

                // 控制并发数量
                if (tasks.Count >= concurrency) {
                    Task.WaitAny(tasks.ToArray());
                    tasks.RemoveAll(t => t.IsCompleted);
                }
            }

            Task.WaitAll(tasks.ToArray());

            return devices;
        }

        private static Device GetDeviceInfo(string ipAddress, int timeout) {
            try {
                using (Ping ping = new Ping()) {
                    PingReply reply = ping.Send(ipAddress, timeout);
                    if (reply.Status == IPStatus.Success) {
                        PhysicalAddress macAddress = ARP.GetMacAddress(IPAddress.Parse(ipAddress));
                        return new Device { IPAddress = ipAddress, MACAddress = macAddress };
                    }
                }
            } catch (PingException) {
                // Ignore Ping exceptions
            }

            return null;
        }

        public static string GetLocalIPAddress() {
            string hostName = Dns.GetHostName();
            IPAddress[] ipAddresses = Dns.GetHostEntry(hostName).AddressList;

            foreach (IPAddress ip in ipAddresses) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }

            return "";
        }
    }

    public class Device {
        public string IPAddress { get; set; }
        public PhysicalAddress MACAddress { get; set; }
    }




    class Program {
        static string deviceIPAddress = "192.168.1.106"; // 要监视的设备的 IP 地址
        static bool isScreenLocked = false;
        static int consecutiveOfflineCount = 0;
        static int maxConsecutiveOfflineCount = 6;


        static async Task Main(string[] args) {
            Console.WriteLine("开始监视设备在线状态...");
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            await CheckDeviceStatusLoop();
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        static async Task CheckDeviceStatusLoop() {
            while (true) {
                bool isDeviceOnline = await CheckDeviceStatus();
                Console.WriteLine("Device Online:" + isDeviceOnline + " isScreenLocked:" + isScreenLocked + " consecutiveOfflineCount:" + consecutiveOfflineCount);
                if (isDeviceOnline) {
                    if (isScreenLocked) {
                        // 解锁屏幕时重置计数
                        consecutiveOfflineCount = 0;
                        isScreenLocked = false;
                        Console.WriteLine($"设备 {deviceIPAddress} 在线，解锁屏幕，重置计数");
                    }
                } else {
                    consecutiveOfflineCount++;
                    if (consecutiveOfflineCount >= maxConsecutiveOfflineCount && !isScreenLocked) {
                        LockWorkStation();
                    }
                }

                await Task.Delay(3000); // 5 秒检测一次
            }
        }

        static async Task<bool> CheckDeviceStatus() {
            try {
                Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync(deviceIPAddress, 1000);
                return reply.Status == IPStatus.Success;
            } catch (PingException) {
                return false;
            }
        }

        static void LockWorkStation() {
            Console.WriteLine($"设备 {deviceIPAddress} 不在线，正在锁定屏幕...");
            isScreenLocked = true;
            // 调用 Windows API 锁定屏幕
            Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
        }

        static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
            if (e.Reason == SessionSwitchReason.SessionUnlock) {
                consecutiveOfflineCount = 0; // 解锁屏幕时重置计数
                isScreenLocked = false;
                Console.WriteLine("屏幕解锁，重置计数");
            } else if (e.Reason == SessionSwitchReason.SessionLock) {
                Console.WriteLine("屏幕锁定");
            }
        }
    }



}