using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration.Pnp;
using Windows.Networking.Sockets;


namespace BluetoothDemo {
    class Program {
        static async Task Main(string[] args) {
            //List<string> connectedDevices = await GetConnectedBluetoothDevices();

            //Console.WriteLine("已连接的蓝牙设备列表：");
            //foreach (string device in connectedDevices) {
            //    Console.WriteLine(device);
            //}
            //List<string> pairedDevices = await GetPairedBluetoothDevices();

            //Console.WriteLine("已配对的蓝牙设备列表：");
            //foreach (string device in pairedDevices) {
            //    Console.WriteLine(device);
            //}

            //List<string> connectedPairedDevices = await GetConnectedPairedNonBleBluetoothDevices();

            //Console.WriteLine("已配对的非BLE蓝牙设备列表：");
            //foreach (string device in connectedPairedDevices) {
            //    Console.WriteLine(device);
            //}
            //ScanForNonBleBluetoothDevices();

            //Console.WriteLine("正在扫描在线的非BLE蓝牙设备...");
            //Console.ReadLine(); // 等待用户按下回车键
            //BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();

            //// 监听设备广播信息
            //watcher.Received += (sender, e) => {
            //    string bluetoothAddressHex = e.BluetoothAddress.ToString("X"); // 将地址转换为十六进制字符串
            //    Console.WriteLine($"设备名称: {e.Advertisement.LocalName}, 设备地址: 0x{bluetoothAddressHex}");
            //};

            //// 开始监听
            //watcher.Start();

            //Console.WriteLine("正在扫描可发现的蓝牙设备...");
            //Console.ReadLine(); // 等待用户按下回车键

            //// 停止监听
            //watcher.Stop();

            //await ScanForAllBluetoothDevices();

            //Console.WriteLine("正在扫描所有可发现的蓝牙设备...");
            //Console.ReadLine(); // 等待用户按下回车键


            // 设备的蓝牙地址（MAC地址）
            //string targetMacAddress = "8c:aa:ce:af:4c:93"; // 替换为你要监视的蓝牙设备的地址

            //List<string> bluetoothDevices = await ScanAllBluetoothDevices();

            //Console.WriteLine($"扫描到的蓝牙设备列表：");
            //foreach (string device in bluetoothDevices) {
            //    Console.WriteLine(device);
            //}

            //if (bluetoothDevices.Contains(targetMacAddress)) {
            //    Console.WriteLine($"蓝牙设备 {targetMacAddress} 存在。");
            //} else {
            //    Console.WriteLine($"蓝牙设备 {targetMacAddress} 不存在。");
            //}


            List<string> bluetoothDevices = await ScanAllBluetoothDevices();

            string targetMacAddress = "8CAACEAF4C93"; // 替换为你要监视的蓝牙设备的MAC地址

            bool isDeviceFound = false;

            foreach (string device in bluetoothDevices) {
                if (device.Contains(targetMacAddress)) {
                    isDeviceFound = true;
                    break;
                }
            }

            if (isDeviceFound) {
                Console.WriteLine($"蓝牙设备 {targetMacAddress} 存在。");
            } else {
                Console.WriteLine($"蓝牙设备 {targetMacAddress} 不存在。");
            }


        }


        static async Task<List<string>> ScanAllBluetoothDevices() {
            List<string> bluetoothDevices = new List<string>();

            // 获取所有蓝牙设备
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync();

            // 遍历设备并记录MAC地址
            foreach (DeviceInformation deviceInfo in devices) {
                if (deviceInfo.Id.Contains("BTHENUM")) {
                    //Console.WriteLine(deviceInfo.Name);
                    Console.WriteLine(deviceInfo.Id);
                    bluetoothDevices.Add(ExtractBluetoothMacAddress(deviceInfo.Id));
                    var online = await IsDeviceOnlineAsync(deviceInfo.Id);
                    Console.WriteLine(online ? "Online" : "Offline");
                }
                // 获取设备的蓝牙地址
                //ulong bluetoothAddress = ulong.Parse(deviceInfo.Id.Substring(deviceInfo.Id.Length - 12), System.Globalization.NumberStyles.HexNumber);
                //string macAddress = FormatBluetoothAddress(bluetoothAddress);


            }

            return bluetoothDevices;
        }


        static string ExtractBluetoothMacAddress(string deviceInfo) {
            string[] parts = deviceInfo.Split('#');
            string[] macParts = parts[parts.Length - 2].Split('_');
            string macAddress = macParts[macParts.Length - 1].Replace("-", "").ToUpper();
            return macAddress;
        }


        public static async Task<bool> IsDeviceOnlineAsync(string deviceId) {
            try {
                // 获取蓝牙设备信息
                DeviceInformation deviceInfo = await DeviceInformation.CreateFromIdAsync(deviceId);
                if (deviceInfo != null) {
                    // 创建 RFCOMM 服务连接
                    RfcommDeviceService rfcommService = await RfcommDeviceService.FromIdAsync(deviceInfo.Id);
                    if (rfcommService != null) {
                        // 尝试连接设备
                        StreamSocket socket = new StreamSocket();
                        await socket.ConnectAsync(rfcommService.ConnectionHostName, rfcommService.ConnectionServiceName);

                        // 如果连接成功，则设备在线
                        return true;
                    }
                }

                return false; // 设备不在线
            } catch (Exception ex) {
                Console.WriteLine($"Error checking device online status: {ex.Message}");
                return false; // 捕获异常，设备可能不在线或不可用
            }
        }

        static async Task<bool> CheckDeviceOnlineStatus(string bluetoothAddress) {

            try {
                // 获取指定地址的设备信息
                DeviceInformation deviceInfo = await DeviceInformation.CreateFromIdAsync(BluetoothDevice.GetDeviceSelectorFromBluetoothAddress(ulong.Parse(bluetoothAddress.Replace(":", ""), System.Globalization.NumberStyles.HexNumber)));

                if (deviceInfo == null) {
                    // 如果设备信息为空，则表示设备离线
                    return false;
                } else {
                    Console.WriteLine($"设备名称: {deviceInfo.Name}, 设备ID: {deviceInfo.Id}");
                    // 获取设备的连接状态
                    BluetoothDevice device = await BluetoothDevice.FromIdAsync(deviceInfo.Id);
                    return device.ConnectionStatus == BluetoothConnectionStatus.Connected;
                }
            } catch (Exception e) {

                Console.WriteLine(e);
                return false;
            }

        }

        static async Task ScanForAllBluetoothDevices() {
            // 扫描BLE设备
            BluetoothLEAdvertisementWatcher bleWatcher = new BluetoothLEAdvertisementWatcher();
            bleWatcher.Received += (sender, e) => {
                Console.WriteLine($"BLE设备名称: {e.Advertisement.LocalName}, 设备地址: 0x{e.BluetoothAddress:X}");
            };
            bleWatcher.Start();

            // 扫描非BLE设备
            DeviceInformationCollection nonBleDevices = await DeviceInformation.FindAllAsync();
            foreach (DeviceInformation deviceInfo in nonBleDevices) {
                Console.WriteLine($"非BLE设备名称: {deviceInfo.Name}, 设备ID: {deviceInfo.Id}");
            }
        }

        static async void ScanForNonBleBluetoothDevices() {
            // 获取所有蓝牙设备
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

            // 遍历设备并打印信息
            foreach (DeviceInformation deviceInfo in devices) {
                Console.WriteLine($"名称: {deviceInfo.Name}, ID: {deviceInfo.Id}");
            }
        }

        static async Task<List<string>> GetConnectedPairedNonBleBluetoothDevices() {
            List<string> connectedPairedDevices = new List<string>();

            // 获取所有已配对的蓝牙设备
            DeviceInformationCollection pairedDevices = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));

            foreach (DeviceInformation deviceInfo in pairedDevices) {
                connectedPairedDevices.Add($"名称: {deviceInfo.Name}, ID: {deviceInfo.Id}");
            }

            return connectedPairedDevices;
        }

        static async Task<List<string>> GetConnectedPairedBluetoothDevices() {
            List<string> connectedPairedDevices = new List<string>();

            // 获取所有已配对的蓝牙设备
            DeviceInformationCollection pairedDevices = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true));

            foreach (DeviceInformation deviceInfo in pairedDevices) {
                // 获取已配对设备的BluetoothLEDevice对象
                BluetoothLEDevice bluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);

                // 检查设备是否已连接
                if (bluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected) {
                    connectedPairedDevices.Add($"名称: {deviceInfo.Name}, ID: {deviceInfo.Id}");
                }
            }

            return connectedPairedDevices;
        }

        static async Task<List<string>> GetPairedBluetoothDevices() {
            List<string> pairedDevices = new List<string>();

            // 获取所有已配对的蓝牙设备
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelectorFromPairingState(true));

            foreach (DeviceInformation deviceInfo in devices) {
                pairedDevices.Add($"名称: {deviceInfo.Name}, ID: {deviceInfo.Id}");
            }

            return pairedDevices;
        }

        static async Task<List<string>> GetConnectedBluetoothDevices() {
            List<string> connectedDevices = new List<string>();

            // 获取所有已配对的蓝牙设备
            DeviceInformationCollection pairedDevices = await DeviceInformation.FindAllAsync(BluetoothDevice.GetDeviceSelector());

            foreach (DeviceInformation deviceInfo in pairedDevices) {
                // 获取已配对设备的BluetoothDevice对象
                BluetoothDevice bluetoothDevice = await BluetoothDevice.FromIdAsync(deviceInfo.Id);

                // 检查设备是否已连接
                if (bluetoothDevice.ConnectionStatus == BluetoothConnectionStatus.Connected) {
                    connectedDevices.Add($"名称: {bluetoothDevice.Name}, ID: {bluetoothDevice.DeviceId}");
                }
            }

            return connectedDevices;
        }
    }
}
