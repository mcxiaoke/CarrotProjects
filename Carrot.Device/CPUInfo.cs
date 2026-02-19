using System;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;

namespace Carrot.Device {

    /// <summary>
    /// \class CPUInfo
    /// 捕获计算机上安装的 CPU 的属性。
    /// 它使用 WMI 类 Win32_Processor 中定义的属性子集
    /// 更多信息，请参阅 <see href="https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-processor">Win32_Processor</see>
    /// </summary>
    public class CPUInfo {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CPUInfo() {
        }

        /// <summary>
        /// CPU 标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 在 32 位操作系统上，值为 32；在 64 位操作系统上，值为 64。
        /// </summary>
        public int AddressWidth { get; set; }

        /// <summary>
        /// 平台使用的处理器架构。
        /// </summary>
        public CPU_ARCHITECTURE Architecture { get; set; }

        /// <summary>
        /// 处理器的当前状态。状态变化指示处理器使用情况，
        /// 而不是处理器的物理状况。
        /// </summary>
        public CPU_STATUS CpuStatus { get; set; }

        /// <summary>
        /// 在 32 位处理器上，值为 32；在 64 位处理器上，值为 64。
        /// </summary>
        public int DataWidth { get; set; }

        /// <summary>
        /// 系统上处理器的唯一标识符。
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 处理器系列类型。
        /// </summary>
        public CPU_FAMILY Family { get; set; }

        /// <summary>
        /// 处理器制造商名称。
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// 处理器最大速度，单位 MHz。
        /// </summary>
        public int MaxClockSpeed { get; set; }

        /// <summary>
        /// 制造商设置的此处理器部件号。
        /// </summary>
        public string PartNumber { get; set; }

        /// <summary>
        /// 此处理器的序列号。此值由制造商设置，通常不可更改。
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 处理器的全局唯一标识符。此标识符可能仅在处理器系列中唯一。
        /// </summary>
        public string UniqueId { get; set; }

        /// <summary>
        /// 处理器主要功能。
        /// </summary>
        public int ProcessorType { get; set; }

        /// <summary>
        /// 描述处理器功能的处理器信息。
        /// </summary>
        public string ProcessorId { get; set; }

        /// <summary>
        /// 每个处理器的负载容量，平均到最后一秒。
        /// 处理器负载是指每个处理器在某一时刻的总计算负担。
        /// </summary>
        public int LoadPercentage { get; set; }

        /// <summary>
        /// 处理器当前速度，单位 MHz。
        /// </summary>
        public int CurrentClockSpeed { get; set; }

        /// <summary>
        /// 处理器电压。
        /// </summary>
        public CPU_VOLTAGE CurrentVoltage { get; set; }

        /// <summary>
        /// 当前处理器实例的核心数。
        /// 核心是集成电路上的物理处理器。
        /// </summary>
        public int NumberOfCores { get; set; }

        /// <summary>
        /// 每个处理器插槽启用的核心数。
        /// </summary>
        public int NumberOfEnabledCore { get; set; }

        /// <summary>
        /// 当前处理器实例的逻辑处理器数。
        /// 对于支持超线程的处理器，此值仅包括
        /// 启用了超线程的处理器。
        /// </summary>
        public int NumberOfLogicalProcessors { get; set; }

        /// <summary>
        /// 依赖于架构的系统修订级别。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        ///二级处理器缓存大小。二级缓存是比主 RAM 内存访问速度更快的外部
        /// 内存区域。
        /// </summary>
        public int L2CacheSize { get; set; }

        /// <summary>
        /// 二级处理器缓存时钟速度。
        /// </summary>
        public int L2CacheSpeed { get; set; }

        /// <summary>
        /// 三级处理器缓存大小。
        /// </summary>
        public int L3CacheSize { get; set; }

        /// <summary>
        /// 三级处理器缓存时钟速度。
        /// </summary>
        public int L3CacheSpeed { get; set; }

        /// <summary>
        /// 每个处理器插槽的线程数。
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// 如果为真，固件已启用虚拟化扩展。
        /// </summary>
        public string VirtualizationFirmwareEnabled { get; set; }

        /// <summary>
        /// 此函数解析管理对象结构以提取 CPU 信息字段。
        /// </summary>
        /// <param name="mgtObject">包含不同 CPU 信息字段的管理对象</param>
        /// <returns>成功返回 0，异常返回 -1</returns>
        public int GetCpuInfo(ManagementObject mgtObject) {
            try {
                // Using null-conditional operator and null-coalescing operator for safer and cleaner code
                Id = mgtObject["Name"] != null ? Regex.Replace(mgtObject["Name"].ToString(), @"\s+", " ") : "";
                AddressWidth = int.Parse(mgtObject["AddressWidth"]?.ToString() ?? "0");
                CpuStatus = GetCpuStatus(int.Parse(mgtObject["CpuStatus"]?.ToString() ?? "0"));
                DataWidth = int.Parse(mgtObject["DataWidth"]?.ToString() ?? "0");
                DeviceID = mgtObject["DeviceID"]?.ToString() ?? "";
                Family = GetCpuFamily(int.Parse(mgtObject["Family"]?.ToString() ?? "0"));
                Manufacturer = mgtObject["Manufacturer"]?.ToString() ?? "";
                MaxClockSpeed = int.Parse(mgtObject["MaxClockSpeed"]?.ToString() ?? "0");
                CurrentClockSpeed = int.Parse(mgtObject["CurrentClockSpeed"]?.ToString() ?? "0");
                PartNumber = mgtObject["PartNumber"]?.ToString() ?? "";
                SerialNumber = mgtObject["SerialNumber"]?.ToString().Trim() ?? "";
                UniqueId = mgtObject["UniqueId"]?.ToString() ?? "";
                ProcessorType = int.Parse(mgtObject["ProcessorType"]?.ToString() ?? "0");
                ProcessorId = mgtObject["ProcessorId"]?.ToString() ?? "";
                LoadPercentage = int.Parse(mgtObject["LoadPercentage"]?.ToString() ?? "0");
                Architecture = GetCpuArchitecture(int.Parse(mgtObject["Architecture"]?.ToString() ?? "-1"));

                CurrentVoltage = mgtObject["CurrentVoltage"] == null ? CPU_VOLTAGE.UNKNOWN : GetCpuCurrentVoltage(int.Parse(mgtObject["CurrentVoltage"].ToString()));
                NumberOfLogicalProcessors = int.Parse(mgtObject["NumberOfLogicalProcessors"]?.ToString() ?? "0");
                NumberOfCores = int.Parse(mgtObject["NumberOfCores"]?.ToString() ?? "0");
                NumberOfEnabledCore = int.Parse(mgtObject["NumberOfEnabledCore"]?.ToString() ?? "0");

                Level = int.Parse(mgtObject["Level"]?.ToString() ?? "0");
                L2CacheSize = int.Parse(mgtObject["L2CacheSize"]?.ToString() ?? "-1");
                L2CacheSpeed = int.Parse(mgtObject["L2CacheSpeed"]?.ToString() ?? "-1");
                L3CacheSize = int.Parse(mgtObject["L3CacheSize"]?.ToString() ?? "-1");
                L3CacheSpeed = int.Parse(mgtObject["L3CacheSpeed"]?.ToString() ?? "-1");

                ThreadCount = int.Parse(mgtObject["ThreadCount"]?.ToString() ?? "-1");

                bool.TryParse(mgtObject["VirtualizationFirmwareEnabled"]?.ToString(), out bool virtualFlag);
                VirtualizationFirmwareEnabled = virtualFlag ? "ENABLED" : "DISABLED";

                return 0;
            } catch (Exception ex) {
#if DEBUG
                Console.WriteLine($"Exception Message: {ex.Message}");
#endif
                return -1;
            }
        }

        /// <summary>
        /// 将 CPUInfo 类的属性转换为字符串。
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() {
            var str = new StringBuilder();

            str.AppendLine($"Device ID: {DeviceID}");
            str.AppendLine($"Name: {Id}");
            str.AppendLine($"Current Clock Speed (MHz): {CurrentClockSpeed}, Max. Clock Speed (MHz): {MaxClockSpeed}");
            str.AppendLine($"Architecture: {Architecture}");

            if (!string.IsNullOrEmpty(Manufacturer))
                str.AppendLine($"Manufacturer: {Manufacturer}");

            str.AppendLine($"NumberOfCores: {NumberOfCores}");
            str.AppendLine($"Number Of Logical Processors: {NumberOfLogicalProcessors}");
            str.AppendLine($"Number Of Enabled Core: {NumberOfEnabledCore}");

            return str.ToString();
        }

        /// <summary>
        /// 此函数将 CPU 架构从枚举转换为字符串
        /// </summary>
        /// <param name="architecture">CPU 架构 (int)</param>
        /// <returns>CPU 架构 (string)</returns>
        protected CPU_ARCHITECTURE GetCpuArchitecture(int architecture) {
            return Enum.IsDefined(typeof(CPU_ARCHITECTURE), architecture) ? (CPU_ARCHITECTURE)architecture : CPU_ARCHITECTURE.NONE;
        }

        /// <summary>
        /// 此函数将 CPU 状态从枚举转换为字符串
        /// </summary>
        /// <param name="status">CPU 状态 (int)</param>
        /// <returns>CPU 状态 (string)</returns>
        protected CPU_STATUS GetCpuStatus(int status) {
            return Enum.IsDefined(typeof(CPU_STATUS), status) ? (CPU_STATUS)status : CPU_STATUS.NONE;
        }

        /// <summary>
        /// 此函数将 CPU 系列从枚举转换为字符串
        /// </summary>
        /// <param name="family">CPU 系列 (int)</param>
        /// <returns>CPU 系列 (string)</returns>
        protected CPU_FAMILY GetCpuFamily(int family) {
            return (CPU_FAMILY)family;
        }

        /// <summary>
        /// 此函数将 CPU 电压从枚举转换为字符串
        /// </summary>
        /// <param name="voltage">CPU 电压 (int)</param>
        /// <returns>CPU 电压 (string)</returns>
        protected CPU_VOLTAGE GetCpuCurrentVoltage(int voltage) {
            return (CPU_VOLTAGE)voltage;
        }
    }
}