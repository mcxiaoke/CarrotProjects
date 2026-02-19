using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

// todo 

//给自动锁定添加

//锁定后降低亮度
//解锁后提高亮度

//按照时间自动亮度功能

namespace Carrot.AutoLock;

    /// <summary>
    /// 表示网络设备信息的记录类型
    /// </summary>
    /// <param name="IPAddress">设备的 IP 地址</param>
    /// <param name="MACAddress">设备的 MAC 地址</param>
    public record Device(string? IPAddress, PhysicalAddress? MACAddress);

    /// <summary>
    /// 提供 ARP (Address Resolution Protocol) 相关功能的辅助类
    /// 包含通过 P/Invoke 调用 Windows IP Helper API 的方法
    /// </summary>
    public static class ARP {
        /// <summary>
        /// 发送 ARP 请求以获取指定 IP 地址的 MAC 地址
        /// </summary>
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint destIp, uint srcIp, byte[] pMacAddr, ref uint phyAddrLen);

        /// <summary>
        /// 获取本地计算机的 ARP 表
        /// </summary>
        [DllImport("iphlpapi.dll", SetLastError = true)]
        public static extern int GetIpNetTable(IntPtr pIpNetTable, ref int pdwSize, bool bOrder);

        /// <summary>
        /// 定义 ARP 表中的一行数据结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct MIB_IPNETROW {
            [MarshalAs(UnmanagedType.U4)]
            public int dwIndex;      // 适配器索引
            [MarshalAs(UnmanagedType.U4)]
            public int dwPhysAddrLen; // MAC 地址长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] bPhysAddr;  // MAC 地址字节数组
            [MarshalAs(UnmanagedType.U4)]
            public int dwAddr;        // IP 地址 (整数形式)
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;        // 类型 (例如: 动态, 静态, 无效)
        }

        /// <summary>
        /// 根据 IP 地址获取对应的 MAC 地址
        /// </summary>
        /// <param name="ipAddress">目标 IP 地址</param>
        /// <returns>设备的物理地址 (MAC)，如果获取失败则返回 null</returns>
        public static PhysicalAddress? GetMacAddress(IPAddress ipAddress) {
            byte[] macAddr = new byte[6];
            uint macAddrLen = (uint)macAddr.Length;
            uint destIp = BitConverter.ToUInt32(ipAddress.GetAddressBytes(), 0);
            if (SendARP(destIp, 0, macAddr, ref macAddrLen) == 0) {
                return new PhysicalAddress(macAddr);
            }
            return null;
        }
    }


    /// <summary>
    /// 提供局域网扫描功能的工具类
    /// </summary>
    public static class NetworkScanner {

        /// <summary>
        /// 异步扫描局域网内的设备
        /// </summary>
        /// <param name="baseIP">基准 IP 地址 (用于确定网段)</param>
        /// <param name="startRange">扫描起始 IP 后缀 (默认为 1)</param>
        /// <param name="endRange">扫描结束 IP 后缀 (默认为 255)</param>
        /// <param name="concurrency">最大并发任务数</param>
        /// <param name="timeout">Ping 超时时间 (毫秒)</param>
        /// <returns>发现的设备列表</returns>
        public static async Task<List<Device>> ScanLocalNetworkAsync(string baseIP,
            int startRange = 1, int endRange = 255,
            int concurrency = 50, int timeout = 500) {
            ConcurrentBag<Device> devices = new();

            string[] parts = baseIP.Split('.');
            string subnet = $"{parts[0]}.{parts[1]}.{parts[2]}.";

            var ips = Enumerable.Range(startRange, endRange - startRange + 1)
                .Select(i => subnet + i);

            var options = new ParallelOptions { MaxDegreeOfParallelism = concurrency };

            await Parallel.ForEachAsync(ips, options, async (ip, ct) => {
                var device = await GetDeviceInfoAsync(ip, timeout);
                if (device is not null) {
                    devices.Add(device);
                }
            });

            return devices.ToList();
        }

        /// <summary>
        /// 同步扫描局域网内的设备 (不推荐，建议使用 ScanLocalNetworkAsync)
        /// </summary>
        /// <param name="baseIP">基准 IP 地址</param>
        /// <param name="startRange">起始范围</param>
        /// <param name="endRange">结束范围</param>
        /// <param name="concurrency">并发数</param>
        /// <param name="timeout">超时时间</param>
        /// <returns>设备列表</returns>
        public static List<Device> ScanLocalNetwork(string baseIP,
            int startRange = 1, int endRange = 255,
            int concurrency = 50, int timeout = 500) {
            ConcurrentBag<Device> devices = new();

            // 分割 IP 地址，获取前三个部分
            string[] parts = baseIP.Split('.');
            string subnet = $"{parts[0]}.{parts[1]}.{parts[2]}.";

            // 并发处理
            List<Task> tasks = new();
            for (int i = startRange; i <= endRange; i++) {
                string ipAddress = subnet + i;
                tasks.Add(Task.Run(() => {
                    var device = GetDeviceInfo(ipAddress, timeout);
                    if (device is not null) {
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

            return devices.ToList();
        }

        /// <summary>
        /// 异步获取指定 IP 的设备信息 (Ping + ARP)
        /// </summary>
        /// <param name="ipAddress">目标 IP 地址</param>
        /// <param name="timeout">超时时间 (毫秒)</param>
        /// <returns>设备信息对象，如果无法连接或解析 MAC 则返回 null</returns>
        public static async Task<Device?> GetDeviceInfoAsync(string ipAddress, int timeout) {
            try {
                using Ping ping = new();
                PingReply reply = await ping.SendPingAsync(ipAddress, timeout);
                if (reply.Status == IPStatus.Success) {
                    var macAddress = ARP.GetMacAddress(IPAddress.Parse(ipAddress));
                    return new Device(ipAddress, macAddress);
                }
            } catch (PingException) {
                // 忽略 Ping 异常
            }

            return null;
        }

        /// <summary>
        /// 同步获取设备信息
        /// </summary>
        public static Device? GetDeviceInfo(string ipAddress, int timeout) {
            try {
                using Ping ping = new();
                PingReply reply = ping.Send(ipAddress, timeout);
                if (reply.Status == IPStatus.Success) {
                    var macAddress = ARP.GetMacAddress(IPAddress.Parse(ipAddress));
                    return new Device(ipAddress, macAddress);
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


    /// <summary>
    /// 网络工具类，提供获取在线设备等功能
    /// </summary>
    public static class NetUtils {


        /// <summary>
        /// 获取当前系统中 ARP 表记录的所有在线设备 IP
        /// 使用 GetIpNetTable API 替代传统的 arp -a 命令行调用，提高效率
        /// </summary>
        /// <returns>在线设备的 IP 地址字符串列表</returns>
        public static List<string> GetOnlineDevices() {
            List<string> onlineDevices = new();

            int bytesNeeded = 0;
            // 第一次调用获取所需的缓冲区大小
            int result = ARP.GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);

            if (bytesNeeded <= 0) return onlineDevices;

            IntPtr buffer = Marshal.AllocCoTaskMem(bytesNeeded);
            try {
                // 第二次调用获取实际数据
                result = ARP.GetIpNetTable(buffer, ref bytesNeeded, false);
                if (result == 0) {
                    int entries = Marshal.ReadInt32(buffer);
                    IntPtr current = IntPtr.Add(buffer, 4);

                    for (int i = 0; i < entries; i++) {
                        ARP.MIB_IPNETROW row = Marshal.PtrToStructure<ARP.MIB_IPNETROW>(current);
                        // dwType: 3 (动态), 4 (静态)
                        // 2 = 无效 (Invalid)
                        // 通常我们只需要有效的条目，这里排除了无效条目
                        if (row.dwType != 2) {
                            IPAddress ip = new IPAddress(BitConverter.GetBytes(row.dwAddr));
                            onlineDevices.Add(ip.ToString());
                        }
                        current = IntPtr.Add(current, Marshal.SizeOf<ARP.MIB_IPNETROW>());
                    }
                }
            } finally {
                Marshal.FreeCoTaskMem(buffer);
            }

            return onlineDevices;
        }

        /// <summary>
        /// 验证字符串是否为有效的 IPv4 地址
        /// </summary>
        public static bool IsValidIPv4(string ipAddress) {
            return IPAddress.TryParse(ipAddress, out var address) && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }
    }
