using System;
using System.Management;
using System.Text;

namespace Carrot.Device {

    /// <summary>
    /// \class DiskPartition
    /// 捕获计算机上安装的磁盘分区的属性。
    /// 它使用 WMI 类 Win32_DiskPartition 中定义的属性子集
    /// 更多信息，请参阅 <see href="https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-diskpartition">Win32_DiskPartition</see>
    /// </summary>
    public class DiskPartition {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public DiskPartition() {
        }

        /// <summary>
        /// 磁盘分区标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 磁盘驱动器和分区的唯一标识符 (区别于系统其他部分)。
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 分区的总大小。
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 连续块的总数，每个块的大小由 BlockSize 属性值决定，这些块构成了此存储范围。
        /// </summary>
        public long NumberOfBlocks { get; set; }

        /// <summary>
        /// 对象的当前状态。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 逻辑设备的状态。
        /// </summary>
        public string StatusInfo { get; set; }

        /// <summary>
        /// 范围系统的创建类名称。
        /// </summary>
        public string SystemCreationClassName { get; set; }

        /// <summary>
        /// 范围系统的名称。
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 分区类型。
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 指示计算机是否可以从此分区引导。
        /// </summary>
        public bool Bootable { get; set; }

        /// <summary>
        /// 分区是否为活动分区。从硬盘引导时，操作系统使用活动分区。
        /// </summary>
        public bool BootPartition { get; set; }

        /// <summary>
        /// 如果为真，则这是主分区。
        /// </summary>
        public bool PrimaryPartition { get; set; }

        /// <summary>
        /// 如果为真，则分区信息已更改。
        /// </summary>
        public bool RewritePartition { get; set; }

        /// <summary>
        /// 此函数解析管理对象结构以提取磁盘分区字段。
        /// </summary>
        /// <param name="mgtObject">包含不同磁盘分区字段的管理对象</param>
        /// <returns>成功返回 0，异常返回 -1</returns>
        public int GetDiskPartitionInfo(ManagementObject mgtObject) {
            try {
                Id = mgtObject["Name"]?.ToString() ?? "";
                DeviceID = mgtObject["DeviceID"]?.ToString() ?? "";

                Size = long.Parse(mgtObject["Size"]?.ToString() ?? "-1");
                NumberOfBlocks = long.Parse(mgtObject["NumberOfBlocks"]?.ToString() ?? "-1");

                Status = mgtObject["Status"]?.ToString() ?? "";
                SystemCreationClassName = mgtObject["SystemCreationClassName"]?.ToString() ?? "";
                SystemName = mgtObject["SystemName"]?.ToString() ?? "";
                Type = mgtObject["Type"]?.ToString() ?? "";

                if (mgtObject["Bootable"] != null) {
                    bool.TryParse(mgtObject["Bootable"].ToString(), out bool temp);
                    Bootable = temp;
                }

                if (mgtObject["BootPartition"] != null) {
                    bool.TryParse(mgtObject["BootPartition"].ToString(), out bool temp);
                    BootPartition = temp;
                }

                if (mgtObject["PrimaryPartition"] != null) {
                    bool.TryParse(mgtObject["PrimaryPartition"].ToString(), out bool temp);
                    PrimaryPartition = temp;
                }

                if (mgtObject["RewritePartition"] != null) {
                    bool.TryParse(mgtObject["RewritePartition"].ToString(), out bool temp);
                    RewritePartition = temp;
                }

                return 0;
            } catch (Exception ex) {
#if DEBUG
                Console.WriteLine($"Exception Message: {ex.Message}");
#endif
                return -1;
            }
        }

        /// <summary>
        /// 将 DiskPartition 类的属性转换为字符串。
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() {
            var str = new StringBuilder();

            str.AppendLine($"Name: {Id}");
            str.AppendLine($"Size (Bytes): {Size}");
            str.AppendLine($"Number Of Blocks: {NumberOfBlocks}");

            if (!string.IsNullOrEmpty(Status))
                str.AppendLine($"Partition Status: {Status}");

            str.AppendLine($"Primary Partition: {PrimaryPartition}");

            return str.ToString();
        }
    }
}