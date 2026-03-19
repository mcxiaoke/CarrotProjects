using Carrot.Common;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Carrot.AutoLock;

/// <summary>
/// 设备在线状态变化事件参数
/// </summary>
public class DeviceStatusEventArgs : EventArgs {
    public bool IsOnline { get; }
    public string? IPAddress { get; }
    public string MacAddress { get; }
    public string DetectionMethod { get; }

    public DeviceStatusEventArgs(bool isOnline, string? ipAddress, string macAddress, string detectionMethod) {
        IsOnline = isOnline;
        IPAddress = ipAddress;
        MacAddress = macAddress;
        DetectionMethod = detectionMethod;
    }
}

/// <summary>
/// MAC 地址检测器。
/// 通过 ARP 扫描检测特定 MAC 地址的设备是否在线。
/// 相比 ActiveChecker 的 Ping+ARP 方案，本检测器直接匹配 MAC 地址，不依赖固定 IP。
/// 
/// Difference from ActiveChecker:
/// - ActiveChecker: Ping 指定 IP → 失败后查 ARP 表 (IP 匹配)
/// - MacAddressDetector: 全网 ARP 扫描 → 匹配 MAC 地址 (更可靠，不受 IP 变化影响)
/// </summary>
public class MacAddressDetector : IDisposable {

    #region Windows API - ARP Table

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_IPNETROW {
        public int dwIndex;
        public int dwPhysAddrLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] bPhysAddr;
        public int dwAddr;
        public int dwType;
    }

    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    private static extern int SendARP(uint destIp, uint srcIp, byte[] pMacAddr, ref uint phyAddrLen);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int GetIpNetTable(IntPtr pIpNetTable, ref int pdwSize, bool bOrder);

    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern int FlushIpNetTable(int dwIfIndex);

    #endregion

    /// <summary>
    /// 默认扫描间隔（秒）
    /// </summary>
    public const int DEFAULT_CHECK_INTERVAL = 30;

    /// <summary>
    /// 默认目标 MAC 地址
    /// </summary>
    public const string DEFAULT_TARGET_MAC = "20:3B:34:54:8A:1C";

    /// <summary>
    /// 默认网段
    /// </summary>
    public const string DEFAULT_SUBNET = "192.168.1.0/24";

    private volatile bool _isRunning;
    private volatile bool _isOnline;
    private string? _lastSeenIP;
    private string _targetMac = DEFAULT_TARGET_MAC;
    private string _subnet = DEFAULT_SUBNET;
    private int _checkInterval = DEFAULT_CHECK_INTERVAL;

    private CancellationTokenSource? _cts;
    private Task? _scanTask;

    /// <summary>
    /// 目标 MAC 地址（格式: XX:XX:XX:XX:XX:XX）
    /// </summary>
    public string TargetMac {
        get => _targetMac;
        set => _targetMac = NormalizeMac(value);
    }

    /// <summary>
    /// 扫描网段（格式: 192.168.1.0/24）
    /// </summary>
    public string Subnet {
        get => _subnet;
        set => _subnet = value;
    }

    /// <summary>
    /// 扫描间隔（秒）
    /// </summary>
    public int CheckInterval {
        get => _checkInterval;
        set => _checkInterval = Math.Max(5, value);
    }

    /// <summary>
    /// 设备是否在线
    /// </summary>
    public bool IsOnline => _isOnline;

    /// <summary>
    /// 最后一次看到的 IP 地址
    /// </summary>
    public string? LastSeenIP => _lastSeenIP;

    /// <summary>
    /// 检测器是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// 设备状态变化事件
    /// </summary>
    public event EventHandler<DeviceStatusEventArgs>? StatusChanged;

    /// <summary>
    /// 状态更新回调（兼容 ActiveChecker 风格）
    /// </summary>
    public Action<bool, string?>? OnStatusUpdate { get; set; }

    public MacAddressDetector() {
        _targetMac = DEFAULT_TARGET_MAC;
        _subnet = DEFAULT_SUBNET;
    }

    public MacAddressDetector(string targetMac, string? subnet = null) {
        _targetMac = NormalizeMac(targetMac);
        if (!string.IsNullOrEmpty(subnet)) {
            _subnet = subnet;
        }
    }

    /// <summary>
    /// 标准化 MAC 地址格式（转大写，去除分隔符）
    /// </summary>
    private static string NormalizeMac(string mac) {
        if (string.IsNullOrWhiteSpace(mac)) return "";
        // 支持多种格式: "20:3B:34:54:8A:1C", "20-3B-34-54-8A-1C", "203B34548A1C"
        var clean = mac.ToUpperInvariant().Replace(":", "").Replace("-", "");
        if (clean.Length != 12) {
            throw new ArgumentException($"Invalid MAC address format: {mac}");
        }
        // 返回标准格式: XX:XX:XX:XX:XX:XX
        return string.Join(":", Enumerable.Range(0, 6).Select(i => clean.Substring(i * 2, 2)));
    }

    /// <summary>
    /// 启动检测
    /// </summary>
    public void Start() {
        if (_isRunning) return;

        Logger.Info($"MacAddressDetector starting, target: {_targetMac}, subnet: {_subnet}");
        _isRunning = true;
        _cts = new CancellationTokenSource();
        _scanTask = Task.Run(() => ScanLoop(_cts.Token));

        OnStatusUpdate?.Invoke(_isOnline, _lastSeenIP);
    }

    /// <summary>
    /// 停止检测
    /// </summary>
    public void Stop() {
        if (!_isRunning) return;

        Logger.Info("MacAddressDetector stopping");
        _isRunning = false;
        _cts?.Cancel();

        try {
            _scanTask?.Wait(2000);
        } catch {
            // Ignore
        }

        OnStatusUpdate?.Invoke(_isOnline, _lastSeenIP);
    }

    /// <summary>
    /// 扫描循环
    /// </summary>
    private async Task ScanLoop(CancellationToken ct) {
        while (_isRunning && !ct.IsCancellationRequested) {
            try {
                var (isOnline, ip) = await ScanByMacAsync(_targetMac, _subnet);

                bool statusChanged = isOnline != _isOnline;

                _isOnline = isOnline;

                if (isOnline && ip != null && ip != _lastSeenIP) {
                    // IP 更新
                    Logger.Info($"IP updated: {_lastSeenIP} -> {ip}");
                    _lastSeenIP = ip;
                } else if (statusChanged) {
                    // 状态变化
                    _lastSeenIP = ip;

                    if (isOnline) {
                        Logger.Info($"Device ONLINE: {_targetMac} @ {ip}");
                    } else {
                        Logger.Info($"Device OFFLINE: {_targetMac}");
                    }

                    StatusChanged?.Invoke(this, new DeviceStatusEventArgs(
                        isOnline, ip, _targetMac, "ARP Scan"));

                    OnStatusUpdate?.Invoke(isOnline, ip);
                }

                await Task.Delay(_checkInterval * 1000, ct);
            } catch (OperationCanceledException) {
                break;
            } catch (Exception ex) {
                Logger.Error("Error in scan loop", ex);
                await Task.Delay(5000, ct);
            }
        }
    }

    /// <summary>
    /// 通过 MAC 地址扫描设备
    /// </summary>
    /// <returns>(是否在线, IP地址)</returns>
    public async Task<(bool isOnline, string? ipAddress)> ScanByMacAsync(string targetMac, string subnet) {
        try {
            // 方法1: 先检查 ARP 缓存
            var (found, ip) = CheckArpCacheForMac(targetMac);
            if (found) {
                return (true, ip);
            }

            // 方法2: 主动扫描网段
            var devices = await ScanSubnetAsync(subnet);

            foreach (var device in devices) {
                if (device.MACAddress != null) {
                    var deviceMac = FormatMac(device.MACAddress);
                    if (deviceMac == NormalizeMac(targetMac)) {
                        return (true, device.IPAddress);
                    }
                }
            }

            return (false, null);
        } catch (Exception ex) {
            Logger.Error($"Scan error for MAC {targetMac}", ex);
            return (false, null);
        }
    }

    /// <summary>
    /// 检查 ARP 缓存中是否有目标 MAC
    /// </summary>
    private (bool found, string? ip) CheckArpCacheForMac(string targetMac) {
        try {
            int bytes = 0;
            GetIpNetTable(IntPtr.Zero, ref bytes, false);

            if (bytes <= 0) return (false, null);

            IntPtr buffer = Marshal.AllocCoTaskMem(bytes);
            try {
                if (GetIpNetTable(buffer, ref bytes, false) == 0) {
                    int count = Marshal.ReadInt32(buffer);
                    IntPtr current = IntPtr.Add(buffer, 4);

                    for (int i = 0; i < count; i++) {
                        var row = Marshal.PtrToStructure<MIB_IPNETROW>(current);

                        if (row.dwPhysAddrLen >= 6 && row.bPhysAddr != null) {
                            var mac = new PhysicalAddress(row.bPhysAddr.Take(6).ToArray());
                            var macStr = FormatMac(mac);

                            if (macStr == NormalizeMac(targetMac)) {
                                // dwAddr 已经是网络字节序，直接转换为 IPAddress
                                var ip = new IPAddress(row.dwAddr);
                                return (true, ip.ToString());
                            }
                        }

                        current = IntPtr.Add(current, Marshal.SizeOf<MIB_IPNETROW>());
                    }
                }
            } finally {
                Marshal.FreeCoTaskMem(buffer);
            }
        } catch (Exception ex) {
            Logger.Debug($"CheckArpCacheForMac error: {ex.Message}");
        }

        return (false, null);
    }

    /// <summary>
    /// 扫描指定网段
    /// </summary>
    public async Task<List<NetworkDevice>> ScanSubnetAsync(string subnet, int timeoutMs = 500, int concurrency = 50) {
        var devices = new ConcurrentBag<NetworkDevice>();

        // 解析网段 "192.168.1.0/24"
        var parts = subnet.Split('/');
        if (parts.Length != 2) {
            Logger.Warning($"Invalid subnet format: {subnet}");
            return devices.ToList();
        }

        var baseIp = parts[0];
        var prefix = int.Parse(parts[1]);

        // 只支持 /24
        if (prefix != 24) {
            Logger.Warning($"Only /24 subnet is supported, got: {prefix}");
        }

        var ipBase = IPAddress.Parse(baseIp);
        var bytes = ipBase.GetAddressBytes();
        var subnetPrefix = $"{bytes[0]}.{bytes[1]}.{bytes[2]}.";

        var options = new ParallelOptions { MaxDegreeOfParallelism = concurrency };

        await Parallel.ForEachAsync(Enumerable.Range(1, 254), options, async (i, ct) => {
            var targetIp = subnetPrefix + i;

            try {
                // Ping
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(targetIp, timeoutMs);

                if (reply.Status == IPStatus.Success) {
                    // 获取 MAC
                    if (IPAddress.TryParse(targetIp, out var addr)) {
                        var mac = GetMacBySendArp(addr);
                        if (mac != null) {
                            devices.Add(new NetworkDevice(targetIp, mac));
                        }
                    }
                }
            } catch {
                // Ignore
            }
        });

        return devices.ToList();
    }

    /// <summary>
    /// 通过 SendARP 获取 MAC 地址
    /// </summary>
    private PhysicalAddress? GetMacBySendArp(IPAddress ipAddress) {
        var ipBytes = ipAddress.GetAddressBytes();
        if (ipBytes.Length != 4) return null;

        uint destIp = BitConverter.ToUInt32(ipBytes, 0);
        byte[] macAddr = new byte[6];
        uint macLen = 6;

        if (SendARP(destIp, 0, macAddr, ref macLen) == 0 && macLen == 6) {
            return new PhysicalAddress(macAddr);
        }

        return null;
    }

    /// <summary>
    /// 格式化 MAC 地址为标准格式
    /// </summary>
    private static string FormatMac(PhysicalAddress mac) {
        var bytes = mac.GetAddressBytes();
        return string.Join(":", bytes.Select(b => b.ToString("X2")));
    }

    public void Dispose() {
        Stop();
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
