using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Carrot.AutoLock;

/// <summary>
/// Represents information about a network device.
/// 表示网络设备信息。
/// </summary>
/// <param name="IPAddress">The device IP address. 设备的 IP 地址。</param>
/// <param name="MACAddress">The device MAC address. 设备的 MAC 地址。</param>
public record NetworkDevice(string? IPAddress, PhysicalAddress? MACAddress);

/// <summary>
/// Provides helper methods for ARP (Address Resolution Protocol).
/// 提供 ARP (地址解析协议) 的辅助方法。
/// </summary>
public static class ArpHelper {
    
    // MIB_IPNETROW structure for GetIpNetTable
    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_IPNETROW {
        [MarshalAs(UnmanagedType.U4)]
        public int dwIndex;
        [MarshalAs(UnmanagedType.U4)]
        public int dwPhysAddrLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] bPhysAddr;
        [MarshalAs(UnmanagedType.U4)]
        public int dwAddr;
        [MarshalAs(UnmanagedType.U4)]
        public int dwType;
    }

    /// <summary>
    /// Sends an ARP request to obtain the MAC address for a specified IP address.
    /// 发送 ARP 请求以获取指定 IP 地址的 MAC 地址。
    /// </summary>
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern int SendARP(uint destIp, uint srcIp, byte[] pMacAddr, ref uint phyAddrLen);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetIpNetTable(IntPtr pIpNetTable, ref int pdwSize, bool bOrder);

    /// <summary>
    /// Gets the MAC address for a given IP address.
    /// 根据 IP 地址获取对应的 MAC 地址。
    /// </summary>
    /// <param name="ipAddress">The target IP address. 目标 IP 地址。</param>
    /// <returns>The physical address (MAC), or null if failed. 设备的物理地址 (MAC)，如果获取失败则返回 null。</returns>
    public static PhysicalAddress? GetMacAddress(IPAddress ipAddress) {
        if (ipAddress == null) return null;

        byte[] macAddr = new byte[6];
        uint macAddrLen = (uint)macAddr.Length;
        byte[] ipBytes = ipAddress.GetAddressBytes();
        
        // Only IPv4 is supported for SendARP
        if (ipBytes.Length != 4) return null;

        uint destIp = BitConverter.ToUInt32(ipBytes, 0);

        if (SendARP(destIp, 0, macAddr, ref macAddrLen) == 0) {
            return new PhysicalAddress(macAddr);
        }
        return null;
    }

    /// <summary>
    /// Gets a list of IP addresses that are currently present in the ARP table (online devices).
    /// 获取 ARP 表中当前的在线设备 IP 地址列表。
    /// </summary>
    public static List<string> GetOnlineDevices() {
        var devices = new List<string>();
        int bytes = 0;
        // First call to get the buffer size
        GetIpNetTable(IntPtr.Zero, ref bytes, false);

        if (bytes <= 0) return devices;

        IntPtr buffer = Marshal.AllocCoTaskMem(bytes);
        try {
            if (GetIpNetTable(buffer, ref bytes, false) == 0) {
                int count = Marshal.ReadInt32(buffer);
                IntPtr current = IntPtr.Add(buffer, 4); // Skip dwNumEntries (4 bytes)

                for (int i = 0; i < count; i++) {
                    MIB_IPNETROW row = Marshal.PtrToStructure<MIB_IPNETROW>(current);
                    
                    // dwAddr is in network byte order (big-endian), need to convert to host byte order
                    // Convert integer IP to string.
                    uint ipAddr = (uint)IPAddress.NetworkToHostOrder((int)row.dwAddr);
                    IPAddress ip = new IPAddress(BitConverter.GetBytes(ipAddr));
                    devices.Add(ip.ToString());

                    current = IntPtr.Add(current, Marshal.SizeOf<MIB_IPNETROW>());
                }
            }
        } catch {
            // Ignore errors
        } finally {
            Marshal.FreeCoTaskMem(buffer);
        }
        return devices;
    }
}

/// <summary>
/// Provides local network scanning capabilities.
/// 提供局域网扫描功能。
/// </summary>
public static class NetworkScanner {

    /// <summary>
    /// Asynchronously scans devices in the local network.
    /// 异步扫描局域网内的设备。
    /// </summary>
    /// <param name="baseIP">The base IP address (e.g. 192.168.1.1). 基准 IP 地址。</param>
    /// <param name="startRange">The start of the range (default 1). 扫描起始 IP 后缀。</param>
    /// <param name="endRange">The end of the range (default 255). 扫描结束 IP 后缀。</param>
    /// <param name="concurrency">The maximum number of concurrent tasks. 最大并发任务数。</param>
    /// <param name="timeout">The Ping timeout in milliseconds. Ping 超时时间 (毫秒)。</param>
    /// <returns>A list of discovered devices. 发现的设备列表。</returns>
    public static async Task<List<NetworkDevice>> ScanLocalNetworkAsync(string baseIP,
        int startRange = 1, int endRange = 255,
        int concurrency = 50, int timeout = 500) {
        
        var devices = new ConcurrentBag<NetworkDevice>();

        // Parse base IP to get subnet
        if (!IPAddress.TryParse(baseIP, out var ip)) {
            return new List<NetworkDevice>();
        }

        byte[] bytes = ip.GetAddressBytes();
        string subnet = $"{bytes[0]}.{bytes[1]}.{bytes[2]}.";

        var ips = Enumerable.Range(startRange, endRange - startRange + 1)
            .Select(i => subnet + i);

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = concurrency };

        await Parallel.ForEachAsync(ips, parallelOptions, async (targetIp, ct) => {
            using var ping = new Ping();
            try {
                var reply = await ping.SendPingAsync(targetIp, timeout);
                if (reply.Status == IPStatus.Success) {
                    // Try to get MAC if reachable
                    if (IPAddress.TryParse(targetIp, out var address)) {
                        var mac = ArpHelper.GetMacAddress(address);
                        devices.Add(new NetworkDevice(targetIp, mac));
                    }
                }
            } catch {
                // Ignore ping failures
            }
        });

        return devices.ToList();
    }
}
