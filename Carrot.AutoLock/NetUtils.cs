using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Carrot.AutoLock {

    public class Device {
        public string? IPAddress { get; set; }
        public PhysicalAddress? MACAddress { get; set; }
    }

    public class ARP {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int destIp, int srcIp, byte[] pMacAddr, ref uint phyAddrLen);

        public static PhysicalAddress? GetMacAddress(IPAddress ipAddress) {
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
            List<Device> devices = new();

            // 分割 IP 地址，获取前三个部分
            string[] parts = baseIP.Split('.');
            string subnet = parts[0] + "." + parts[1] + "." + parts[2] + ".";

            // 并发处理
            List<Task> tasks = new();
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

        public static Device? GetDeviceInfo(string ipAddress, int timeout) {
            try {
                using Ping ping = new();
                PingReply reply = ping.Send(ipAddress, timeout);
                if (reply.Status == IPStatus.Success) {
                    var macAddress = ARP.GetMacAddress(IPAddress.Parse(ipAddress));
                    return new Device { IPAddress = ipAddress, MACAddress = macAddress };
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


    public class NetUtils {


        public static List<string> GetOnlineDevices() {
            List<string> onlineDevices = new();

            Process process = new();
            process.StartInfo.FileName = "arp";
            process.StartInfo.Arguments = "-a";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            string[] lines = output.Split('\n');
            foreach (string line in lines) {
                if (!string.IsNullOrWhiteSpace(line)) {
                    string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3 && IsValidIPv4(parts[0])) {
                        onlineDevices.Add(parts[0]);
                    }
                }
            }

            return onlineDevices;
        }

        public static bool IsValidIPv4(string ipAddress) {
            IPAddress address;
            return IPAddress.TryParse(ipAddress, out address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }
    }
}
