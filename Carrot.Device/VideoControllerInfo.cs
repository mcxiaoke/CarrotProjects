using System;
using System.Management;
using System.Text;

namespace Carrot.Device {

    /// <summary>
    /// \class VideoControllerInfo
    /// 捕获视频控制器的主要属性。
    /// 它使用 WMI 类 Win32_VideoController 中定义的属性子集
    /// 更多信息，请参阅 <see href="https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-videocontroller">Win32_VideoController</see>
    /// </summary>
    public class VideoControllerInfo {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public VideoControllerInfo() {
        }

        /// <summary>
        /// 视频控制器标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 视频控制器的当前分辨率、颜色和扫描模式设置。
        /// </summary>
        public string VideoModeDescription { get; set; }

        /// <summary>
        /// 描述视频处理器的自由格式字符串。
        /// </summary>
        public string VideoProcessor { get; set; }

        /// <summary>
        /// 范围系统的名称。
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// 对象描述。
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 对象的当前状态。可以定义各种操作和非操作状态。
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 视频适配器的内存大小。
        /// </summary>
        public long AdapterRAM { get; set; }

        /// <summary>
        /// 系统的颜色表的大小。设备每像素的颜色深度不得超过 8 位；
        /// 否则，不会设置此属性。
        /// </summary>
        public int ColorTableEntries { get; set; }

        /// <summary>
        /// 数模转换器 (DAC) 芯片的名称或标识符。
        /// 此属性的字符集是字母数字。
        /// </summary>
        public string AdapterDACType { get; set; }

        /// <summary>
        /// 逻辑设备报告的最后一个错误代码。
        /// </summary>
        public int LastErrorCode { get; set; }

        /// <summary>
        /// 支持的最大内存量（字节）。
        /// </summary>
        public int MaxMemorySupported { get; set; }

        /// <summary>
        /// 此控制器支持的可直接寻址实体的最大数量。
        /// 如果数量未知，应使用值 0（零）。
        /// </summary>
        public int MaxNumberControlled { get; set; }

        /// <summary>
        /// 视频控制器的最大刷新率（赫兹）。
        /// </summary>
        public int MaxRefreshRate { get; set; }

        /// <summary>
        /// 视频控制器的最小刷新率（赫兹）。
        /// </summary>
        public int MinRefreshRate { get; set; }

        /// <summary>
        /// 视频架构类型。
        /// </summary>
        public VIDEO_ARCHITECTURE VideoArchitecture { get; set; }

        /// <summary>
        /// 视频内存类型。
        /// </summary>
        public VIDEO_MEMORY_TYPE VideoMemoryType { get; set; }

        /// <summary>
        /// 当前视频模式。
        /// </summary>
        public int VideoMode { get; set; }

        /// <summary>
        /// 用于显示每个像素的位数。
        /// </summary>
        public int CurrentBitsPerPixel { get; set; }

        /// <summary>
        /// 当前水平像素数。
        /// </summary>
        public int CurrentHorizontalResolution { get; set; }

        /// <summary>
        /// 当前分辨率支持的颜色数。
        /// </summary>
        public long CurrentNumberOfColors { get; set; }

        /// <summary>
        /// 此视频控制器的列数（如果在字符模式下）。
        /// </summary>
        public long CurrentNumberOfColumns { get; set; }

        /// <summary>
        /// 此视频控制器的行数（如果在字符模式下）。
        /// </summary>
        public long CurrentNumberOfRows { get; set; }

        /// <summary>
        /// 视频控制器刷新显示器图像的频率。
        /// </summary>
        public int CurrentRefreshRate { get; set; }

        /// <summary>
        /// 当前扫描模式。
        /// </summary>
        public int CurrentScanMode { get; set; }

        /// <summary>
        /// 当前垂直像素数。
        /// </summary>
        public int CurrentVerticalResolution { get; set; }

        /// <summary>
        /// 当前特定于设备的笔数。
        /// </summary>
        public int DeviceSpecificPens { get; set; }

        /// <summary>
        /// 视频控制器的抖动类型。
        /// </summary>
        public int DitherType { get; set; }

        /// <summary>
        /// 当前安装的视频驱动程序的最后修改日期和时间。
        /// </summary>
        public string DriverDate { get; set; }

        /// <summary>
        /// 此函数解析管理对象结构以提取视频控制器信息字段。
        /// </summary>
        /// <param name="mgtObject">包含不同视频控制器信息字段的管理对象</param>
        /// <returns>成功返回 0，异常返回 -1</returns>
        public int GetVideoControllerInfo(ManagementObject mgtObject) {
            try {
                CurrentBitsPerPixel = int.Parse(mgtObject["CurrentBitsPerPixel"]?.ToString() ?? "-1");
                CurrentHorizontalResolution = int.Parse(mgtObject["CurrentHorizontalResolution"]?.ToString() ?? "-1");
                CurrentNumberOfColors = long.Parse(mgtObject["CurrentNumberOfColors"]?.ToString() ?? "-1");
                CurrentNumberOfColumns = long.Parse(mgtObject["CurrentNumberOfColumns"]?.ToString() ?? "-1");
                CurrentNumberOfRows = long.Parse(mgtObject["CurrentNumberOfRows"]?.ToString() ?? "-1");
                CurrentRefreshRate = int.Parse(mgtObject["CurrentRefreshRate"]?.ToString() ?? "-1");

                CurrentScanMode = int.Parse(mgtObject["CurrentScanMode"]?.ToString() ?? "-1");
                CurrentVerticalResolution = int.Parse(mgtObject["CurrentVerticalResolution"]?.ToString() ?? "-1");
                DeviceSpecificPens = int.Parse(mgtObject["DeviceSpecificPens"]?.ToString() ?? "-1");

                DitherType = int.Parse(mgtObject["DitherType"]?.ToString() ?? "-1");

                Id = mgtObject["Name"]?.ToString() ?? "";

                VideoModeDescription = mgtObject["VideoModeDescription"]?.ToString() ?? "";
                VideoProcessor = mgtObject["VideoProcessor"]?.ToString() ?? "";
                SystemName = mgtObject["SystemName"]?.ToString() ?? "";
                Description = mgtObject["Description"]?.ToString() ?? "";
                Status = mgtObject["Status"]?.ToString() ?? "";

                AdapterRAM = long.Parse(mgtObject["AdapterRAM"]?.ToString() ?? "-1");
                ColorTableEntries = int.Parse(mgtObject["ColorTableEntries"]?.ToString() ?? "-1");
                AdapterDACType = mgtObject["AdapterDACType"]?.ToString() ?? "";

                LastErrorCode = int.Parse(mgtObject["LastErrorCode"]?.ToString() ?? "-1");
                MaxMemorySupported = int.Parse(mgtObject["MaxMemorySupported"]?.ToString() ?? "-1");
                MaxNumberControlled = int.Parse(mgtObject["MaxNumberControlled"]?.ToString() ?? "-1");
                MaxRefreshRate = int.Parse(mgtObject["MaxRefreshRate"]?.ToString() ?? "-1");
                MinRefreshRate = int.Parse(mgtObject["MinRefreshRate"]?.ToString() ?? "-1");

                VideoArchitecture = GetVideoArchitecture(int.Parse(mgtObject["VideoArchitecture"]?.ToString() ?? "2"));
                VideoMemoryType = GetVideoMemoryType(int.Parse(mgtObject["VideoMemoryType"]?.ToString() ?? "2"));
                VideoMode = int.Parse(mgtObject["VideoMode"]?.ToString() ?? "-1");

                if (mgtObject["DriverDate"] != null) {
                    string str = mgtObject["DriverDate"].ToString().TrimEnd();
                    if (str.Length >= 14) // Ensure string is long enough
                    {
                        int year = int.Parse(str.Substring(0, 4));
                        int month = int.Parse(str.Substring(4, 2));
                        int day = int.Parse(str.Substring(6, 2));

                        int hour = int.Parse(str.Substring(8, 2));
                        int minute = int.Parse(str.Substring(10, 2));
                        int second = int.Parse(str.Substring(12, 2));

                        DateTime date = new DateTime(year, month, day, hour, minute, second);
                        DriverDate = date.ToLocalTime().ToString();
                    }
                    else 
                    {
                        DriverDate = str;
                    }
                } else {
                    DriverDate = "";
                }

                return 0;
            } catch (Exception ex) {
#if DEBUG
                Console.WriteLine($"Exception Message: {ex.StackTrace}");
#endif
                return -1;
            }
        }

        /// <summary>
        /// 此函数将视频控制器架构从枚举转换为字符串
        /// </summary>
        /// <param name="architecture">视频控制器架构 (int)</param>
        /// <returns>视频控制器架构 (string)</returns>
        protected VIDEO_ARCHITECTURE GetVideoArchitecture(int architecture) {
            return (VIDEO_ARCHITECTURE)architecture;
        }

        /// <summary>
        /// 此函数将视频内存类型从枚举转换为字符串
        /// </summary>
        /// <param name="memtype">视频内存类型 (int)</param>
        /// <returns>视频内存类型 (string)</returns>
        protected VIDEO_MEMORY_TYPE GetVideoMemoryType(int memtype) {
            return (VIDEO_MEMORY_TYPE)memtype;
        }

        /// <summary>
        /// 将 VideoControllerInfo 类的属性转换为字符串。
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() {
            var str = new StringBuilder();

            str.AppendLine($"Name: {Id}");
            str.AppendLine($"Video Architecture: {VideoArchitecture}");
            str.AppendLine($"Video Memory Type: {VideoMemoryType}");
            str.AppendLine($"Video Controller Status: {Status}");
            str.AppendLine($"Adapter RAM (Bytes): {AdapterRAM}");
            str.AppendLine($"Driver Date: {DriverDate}");
            str.AppendLine($"Video Mode Description: {VideoModeDescription}");

            return str.ToString();
        }
    }
}