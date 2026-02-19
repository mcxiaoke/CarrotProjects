using System;
using System.Management;
using System.Text;

namespace Carrot.Device {

    /// <summary>
    /// \class DiskDriveInfo
    /// 捕获计算机上安装的磁盘驱动器的属性。
    /// 它使用 WMI 类 Win32_DiskDrive 中定义的属性子集
    /// 更多信息，请参阅 <see href="https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-diskdrive">Win32_DiskDrive</see>
    /// </summary>
    public class DiskDriveInfo {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public DiskDriveInfo() {
        }

        /// <summary>
        /// 磁盘驱动器标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 与系统上其他设备一起的磁盘驱动器的唯一标识符。
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 制造商的磁盘驱动器型号。
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 制造商分配的用于识别物理介质的编号。
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 制造商分配的用于识别物理介质的编号。
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// 磁盘标识。此属性可用于标识共享资源。
        /// </summary>
        public int Signature { get; set; }

        /// <summary>
        /// 对象的当前状态。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 逻辑设备的状态。
        /// </summary>
        public string StatusInfo { get; set; }

        /// <summary>
        /// 范围计算机的 CreationClassName 属性的值。
        /// </summary>
        public string SystemCreationClassName { get; set; }

        /// <summary>
        /// 范围系统的名称。
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 物理磁盘驱动器上的柱面总数。
        /// </summary>
        public long TotalCylinders { get; set; }

        /// <summary>
        /// 磁盘驱动器上的磁头总数。
        /// </summary>
        public long TotalHeads { get; set; }

        /// <summary>
        /// 物理磁盘驱动器上的扇区总数。
        /// </summary>
        public long TotalSectors { get; set; }

        /// <summary>
        /// 物理磁盘驱动器上的磁道总数。
        /// </summary>
        public long TotalTracks { get; set; }

        /// <summary>
        /// 磁盘驱动器的大小。
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 支持或插入的最大介质数。
        /// </summary>
        public int NumberOfMediaSupported { get; set; }

        /// <summary>
        /// 此物理磁盘驱动器上被操作系统识别的分区数。
        /// </summary>
        public int Partitions { get; set; }

        /// <summary>
        /// 物理磁盘驱动器上每个柱面的磁道数。
        /// </summary>
        public int TracksPerCylinder { get; set; }

        /// <summary>
        /// 此函数解析管理对象结构以提取磁盘驱动器信息字段。
        /// </summary>
        /// <param name="mgtObject">包含不同磁盘驱动器信息字段的管理对象</param>
        /// <returns>成功返回 0，异常返回 -1</returns>
        public int GetDiskDriveInfo(ManagementObject mgtObject) {
            try {
                Id = mgtObject["Name"]?.ToString() ?? "";
                DeviceID = mgtObject["DeviceID"]?.ToString() ?? "";
                Model = mgtObject["Model"]?.ToString() ?? "";
                Manufacturer = mgtObject["Manufacturer"]?.ToString() ?? "";

                SerialNumber = mgtObject["SerialNumber"]?.ToString().Trim() ?? "";
                Status = mgtObject["Status"]?.ToString() ?? "";
                SystemCreationClassName = mgtObject["SystemCreationClassName"]?.ToString() ?? "";
                SystemName = mgtObject["SystemName"]?.ToString() ?? "";

                TotalCylinders = long.Parse(mgtObject["TotalCylinders"]?.ToString() ?? "-1");
                TotalHeads = long.Parse(mgtObject["TotalHeads"]?.ToString() ?? "-1");
                TotalSectors = long.Parse(mgtObject["TotalSectors"]?.ToString() ?? "-1");
                TotalTracks = long.Parse(mgtObject["TotalTracks"]?.ToString() ?? "-1");
                Size = long.Parse(mgtObject["Size"]?.ToString() ?? "-1");

                NumberOfMediaSupported = int.Parse(mgtObject["NumberOfMediaSupported"]?.ToString() ?? "-1");
                Partitions = int.Parse(mgtObject["Partitions"]?.ToString() ?? "-1");
                StatusInfo = GetStatusInfo(int.Parse(mgtObject["StatusInfo"]?.ToString() ?? "-1"));
                TracksPerCylinder = int.Parse(mgtObject["TracksPerCylinder"]?.ToString() ?? "-1");
                //Signature = (mgtObject["Signature"] == null) ? -1 : int.Parse(mgtObject["Signature"].ToString());

                return 0;
            } catch (Exception ex) {
#if DEBUG
                Console.WriteLine($"Exception Message: {ex.Message}");
#endif
                return -1;
            }
        }

        /// <summary>
        /// 此函数将磁盘驱动器状态从枚举转换为字符串
        /// </summary>
        /// <param name="status">磁盘驱动器状态 (int)</param>
        /// <returns>磁盘驱动器状态 (string)</returns>
        protected string GetStatusInfo(int status) {
            return status switch {
                1 => "OTHER",
                2 => "UNKNOWN",
                3 => "ENABLED",
                4 => "DISABLED",
                5 => "NOT APPLICABLE",
                _ => "",
            };
        }

        /// <summary>
        /// 将 DiskDriveInfo 类的属性转换为字符串。
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() {
            var str = new StringBuilder();

            str.AppendLine($"Name: {Id}");

            if (!string.IsNullOrEmpty(Manufacturer))
                str.AppendLine($"Manufacturer: {Manufacturer}");

            if (!string.IsNullOrEmpty(SerialNumber))
                str.AppendLine($"SerialNumber: {SerialNumber}");

            if (!string.IsNullOrEmpty(Model))
                str.AppendLine($"Model: {Model}");

            if (Size >= 0)
                str.AppendLine($"Size (Bytes): {Size}");

            str.AppendLine($"Partitions: {Partitions}");

            if (!string.IsNullOrEmpty(Status))
                str.AppendLine($"Status: {Status}");

            return str.ToString();
        }
    }
}