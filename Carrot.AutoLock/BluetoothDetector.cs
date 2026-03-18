using Carrot.Common;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;

namespace Carrot.AutoLock;

/// <summary>
/// 蓝牙设备检测器 (适用于 .NET 10 + Windows SDK)
/// 封装了两种免 App 的蓝牙检测方案：已配对状态查询、BLE 广播扫描。
/// </summary>
public class BluetoothDetector : IDisposable {
    // 用于保存扫描到的 BLE 设备：Key 为 MAC 地址或设备名，Value 为 (最后发现时间, 信号强度)
    private readonly ConcurrentDictionary<string, (DateTime LastSeen, short Rssi)> _nearbyDevices;
    private BluetoothLEAdvertisementWatcher? _bleWatcher;
    private bool _isScanning;

    public BluetoothDetector() {
        _nearbyDevices = new ConcurrentDictionary<string, (DateTime, short)>(StringComparer.OrdinalIgnoreCase);
    }

    #region 方案一：查询已配对设备的连接状态 (最稳，但要求手机系统未主动断开)

    /// <summary>
    /// 检测指定的已配对蓝牙设备当前是否处于“已连接”状态。
    /// 原理：查询 Windows 设备管理器中该蓝牙设备的状态。
    /// 优点：最准确，完全不受 MAC 随机化影响。
    /// 缺点：手机为了省电，长时间无数据传输时可能会主动断开蓝牙连接。
    /// </summary>
    /// <param name="deviceName">设备在 Windows 蓝牙列表中的确切名称 (如 "iPhone 15 Pro" 或 "Mi Band 8")</param>
    /// <returns>是否已连接</returns>
    public async Task<bool> IsPairedDeviceConnectedAsync(string deviceName) {
        try {
            // 通过设备名称获取蓝牙设备的筛选器
            string selector = BluetoothDevice.GetDeviceSelectorFromDeviceName(deviceName);
            var deviceInformationCollection = await DeviceInformation.FindAllAsync(selector);

            if (deviceInformationCollection.Count == 0)
                return false;

            // 遍历匹配的设备，检查连接状态
            foreach (var deviceInfo in deviceInformationCollection) {
                using var bluetoothDevice = await BluetoothDevice.FromIdAsync(deviceInfo.Id);
                if (bluetoothDevice != null && bluetoothDevice.ConnectionStatus == BluetoothConnectionStatus.Connected) {
                    return true;
                }
            }
        } catch (Exception ex) {
            Logger.Error("[BT] Paired device check error", ex);
        }
        return false;
    }

    #endregion

    #region 方案二：BLE 低功耗广播扫描 (雷达模式，支持估算距离)

    /// <summary>
    /// 启动 BLE 后台扫描雷达。
    /// 原理：监听周围所有 BLE 设备发出的广播包 (Advertisement)。
    /// 优点：不需要建立实际连接，哪怕手机处于待机状态，只要开启了蓝牙就会偶尔广播。
    /// </summary>
    public void StartBleScanner() {
        if (_isScanning) return;

        _bleWatcher = new BluetoothLEAdvertisementWatcher {
            ScanningMode = BluetoothLEScanningMode.Active
        };

        _bleWatcher.Received += OnAdvertisementReceived;
        _bleWatcher.Stopped += OnScannerStopped;
        _bleWatcher.Start();
        _isScanning = true;

        Logger.Info("[BT] BLE Scanner started");
    }

    /// <summary>
    /// 扫描器停止时的回调
    /// </summary>
    private void OnScannerStopped(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementWatcherStoppedEventArgs args) {
        if (args.Error != BluetoothError.Success) {
            Logger.Warning($"[BT] BLE Scanner stopped with error: {args.Error}");
        }
        _isScanning = false;
    }

    /// <summary>
    /// 停止 BLE 后台扫描。
    /// </summary>
    public void StopBleScanner() {
        if (!_isScanning || _bleWatcher == null) return;

        _bleWatcher.Received -= OnAdvertisementReceived;
        _bleWatcher.Stopped -= OnScannerStopped;
        _bleWatcher.Stop();
        _bleWatcher = null;
        _isScanning = false;
        _nearbyDevices.Clear();

        Logger.Info("[BT] BLE Scanner stopped");
    }

    /// <summary>
    /// 接收到 BLE 广播包时的回调处理
    /// </summary>
    private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args) {
        var timestamp = DateTime.Now;
        var rssi = args.RawSignalStrengthInDBm;
        var macAddress = FormatMacAddress(args.BluetoothAddress);
        var localName = args.Advertisement.LocalName;

        // 记录 MAC 地址的状态
        _nearbyDevices[macAddress] = (timestamp, rssi);

        // 如果设备广播了名称，也按名称记录一份
        if (!string.IsNullOrEmpty(localName)) {
            _nearbyDevices[localName] = (timestamp, rssi);
        }
    }

    /// <summary>
    /// 检查指定设备近期是否在电脑附近 (基于 BLE 雷达)。
    /// </summary>
    /// <param name="targetId">要查找的设备的 MAC 地址(如 00:11:22:33:44:55) 或 广播名称(如 MyPhone)</param>
    /// <param name="timeoutSeconds">离线判定时间(秒)。例如：最近 30 秒内没收到信号就算作离开</param>
    /// <param name="minRssi">最小信号强度阈值。例如设为 -85，低于 -85dBm 说明人走远了</param>
    /// <returns>设备是否在有效范围内</returns>
    public bool IsDeviceNearby(string targetId, int timeoutSeconds = 30, short minRssi = -85) {
        if (!_isScanning) return false;

        // 清理一下过期的缓存数据，防止内存无限增长
        CleanupOldDevices(timeoutSeconds * 2);

        if (_nearbyDevices.TryGetValue(targetId, out var deviceData)) {
            bool isRecent = (DateTime.Now - deviceData.LastSeen).TotalSeconds <= timeoutSeconds;
            bool isSignalStrongEnough = deviceData.Rssi >= minRssi;

            return isRecent && isSignalStrongEnough;
        }

        return false;
    }

    #endregion

    #region 辅助工具方法

    /// <summary>
    /// 将 ulong 格式的蓝牙地址格式化为标准的 MAC 字符串 (AA:BB:CC:DD:EE:FF)
    /// </summary>
    private string FormatMacAddress(ulong address) {
        var tempMac = address.ToString("X12"); // 16进制，补齐12位
        return string.Join(":", Enumerable.Range(0, 6).Select(i => tempMac.Substring(i * 2, 2)));
    }

    /// <summary>
    /// 清理长期未见到的设备数据
    /// </summary>
    private void CleanupOldDevices(int expireSeconds) {
        var now = DateTime.Now;
        var expiredKeys = _nearbyDevices.Where(kv => (now - kv.Value.LastSeen).TotalSeconds > expireSeconds)
                                        .Select(kv => kv.Key)
                                        .ToList();

        foreach (var key in expiredKeys) {
            _nearbyDevices.TryRemove(key, out _);
        }
    }

    public void Dispose() {
        StopBleScanner();
    }

    #endregion
}