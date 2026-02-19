using System;
using System.Management;

namespace Carrot.Device {

    /// <summary>
    /// \class MemoryBankInfo
    /// 捕获 MemoryBankInfo 结构的主要属性。
    /// 它使用 WMI 类 Win32_PhysicalMemory 中定义的属性子集。
    /// 更多信息，请参阅 <see href="https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-physicalmemory">Win32_PhysicalMemory 类</see>
    /// </summary>
    public class MemoryBankInfo {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public MemoryBankInfo() {
        }

        /// <summary>
        /// 内存条信息标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 内存所在的物理标记插槽。
        /// </summary>
        public string BankLabel { get; set; }

        /// <summary>
        /// 物理内存的数据宽度（位）。
        /// </summary>
        public int DataWidth { get; set; }

        /// <summary>
        /// 对象描述。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 容纳内存的插座或电路板的标签。
        /// </summary>
        public string DeviceLocator { get; set; }

        /// <summary>
        /// 负责生产物理元件的组织名称。
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// 制造商分配的用于识别物理元件的编号。
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 物理元件的库存单位编号。
        /// </summary>
        public string SKU { get; set; }

        /// <summary>
        /// 原始 SMBIOS 内存类型。
        /// </summary>
        public int SMBIOSMemoryType { get; set; }

        /// <summary>
        /// 物理内存的速度（纳秒）。
        /// </summary>
        public int Speed { get; set; }

        /// <summary>
        /// 对象的当前状态。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 物理元件的名称。
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// 除资产标签信息外，可用于识别物理元件的其他数据。
        /// </summary>
        public string OtherIdentifyingInfo { get; set; }

        /// <summary>
        /// 负责生产或制造物理元件的组织分配的部件号。
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// Win32_PhysicalMemory 实例所表示的物理内存设备的唯一标识符。
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// 物理内存的总宽度（位），包括校验位或纠错位。
        /// </summary>
        public int TotalWidth { get; set; }

        /// <summary>
        /// 表示的物理内存类型。
        /// </summary>
        public int TypeDetail { get; set; }

        /// <summary>
        /// 物理元件的版本。
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 物理内存所在行的位置。
        /// </summary>
        public int PositionInRow { get; set; }

        /// <summary>
        /// 芯片的实现外形尺寸。
        /// </summary>
        public MEM_BANK_FORM_FACTOR FormFactor { get; set; }

        /// <summary>
        /// 物理内存的总容量（字节）。
        /// </summary>
        public long Capacity { get; set; }

        /// <summary>
        /// 此函数解析管理对象结构以提取内存条信息字段。
        /// </summary>
        /// <param name="mgtObject">包含不同内存条信息字段的管理对象</param>
        /// <returns>如果成功则返回 0，如果发生异常则返回 -1</returns>
        public int GetMemInfo(ManagementObject mgtObject) {
            try {
                Capacity = long.Parse(mgtObject["Capacity"]?.ToString() ?? "0");

                Id = mgtObject["Name"]?.ToString() ?? "";
                BankLabel = mgtObject["BankLabel"]?.ToString() ?? "";
                Description = mgtObject["Description"]?.ToString() ?? "";
                DeviceLocator = mgtObject["DeviceLocator"]?.ToString() ?? "";
                Manufacturer = mgtObject["Manufacturer"]?.ToString() ?? "";

                SerialNumber = mgtObject["SerialNumber"]?.ToString() ?? "";
                SKU = mgtObject["SKU"]?.ToString() ?? "";
                Status = mgtObject["Status"]?.ToString() ?? "";
                Model = mgtObject["Model"]?.ToString() ?? "";
                OtherIdentifyingInfo = mgtObject["OtherIdentifyingInfo"]?.ToString() ?? "";
                PartNumber = mgtObject["PartNumber"]?.ToString() ?? "";

                DataWidth = int.Parse(mgtObject["DataWidth"]?.ToString() ?? "0");
                Speed = int.Parse(mgtObject["Speed"]?.ToString() ?? "0");
                SMBIOSMemoryType = int.Parse(mgtObject["SMBIOSMemoryType"]?.ToString() ?? "0");

                Tag = mgtObject["Tag"]?.ToString() ?? "";
                Version = mgtObject["Version"]?.ToString() ?? "";
                TotalWidth = int.Parse(mgtObject["TotalWidth"]?.ToString() ?? "0");
                TypeDetail = int.Parse(mgtObject["TypeDetail"]?.ToString() ?? "0");
                PositionInRow = int.Parse(mgtObject["PositionInRow"]?.ToString() ?? "-1");
                FormFactor = GetMemBankFormFactor(int.Parse(mgtObject["FormFactor"]?.ToString() ?? "0"));

                return 0;
            } catch (Exception ex) {
#if DEBUG
                Console.WriteLine($"Exception Message: {ex.Message}");
#endif
                return -1;
            }
        }

        /// <summary>
        /// 此函数将内存条外形尺寸从枚举转换为字符串
        /// </summary>
        /// <param name="formfactor">内存条外形尺寸 (int)</param>
        /// <returns>内存条外形尺寸 (string)</returns>
        protected MEM_BANK_FORM_FACTOR GetMemBankFormFactor(int formfactor) {
            return (MEM_BANK_FORM_FACTOR)formfactor;
        }
    }
}