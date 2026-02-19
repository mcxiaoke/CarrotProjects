using System;
using System.Collections;
using System.Diagnostics;

namespace Carrot.Device {

    /// <summary>
    /// \class PlatformInfo
    /// 捕获计算机平台的属性。
    /// </summary>
    public class PlatformInfo {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public PlatformInfo() {
        }

        /// <summary>
        /// 指示操作系统是否为 64 位的标志
        /// </summary>
        public bool Is64BitOperatingSystem { get; set; }

        /// <summary>
        /// 当前机器名
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// 操作系统名称
        /// </summary>
        public OperatingSystem OS { get; set; }

        /// <summary>
        /// 平台标识符
        /// </summary>
        public PlatformID Platform { get; set; }

        /// <summary>
        /// 已安装的服务包
        /// </summary>
        public string ServicePack { get; set; }

        /// <summary>
        /// 操作系统版本
        /// </summary>
        public string VersionString { get; set; }

        /// <summary>
        /// 计算机处理器数量
        /// </summary>
        public int ProcessorCount { get; set; }

        /// <summary>
        /// 逻辑驱动器列表
        /// </summary>
        public string[] LogicalDrives { get; set; }

        /// <summary>
        /// 包含环境变量列表的字典
        /// </summary>
        public IDictionary EnvVars { get; set; }

        /// <summary>
        /// CLR 版本
        /// </summary>
        public Version ClrVersion { get; set; }

        /// <summary>
        /// 获取系统信息（包括操作系统、逻辑驱动器、环境变量等）
        /// </summary>
        /// <returns>成功返回 0，发生异常返回 -1</returns>
        public int GetSystemInfo() {
            try {
#if DEBUG
                Console.WriteLine($"********** Platform General Information **********");
#endif

                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"********** Platform General Information **********\n", true);
                }

                // 在我的电脑上返回 true，因为它是 64 位操作系统
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;

#if DEBUG
                Console.WriteLine("is64BitOperatingSystem: " + Is64BitOperatingSystem);
#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"is64BitOperatingSystem: {Is64BitOperatingSystem}\n", true);
                }

                // 返回机器名
                MachineName = Environment.MachineName;
#if DEBUG
                Console.WriteLine("machineName: " + MachineName);
#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"machineName: {MachineName}\n", true);
                }

                // 返回有关操作系统版本、内部版本、主要、次要等的信息。
                OS = Environment.OSVersion;

#if DEBUG
                Console.WriteLine("OS: " + OS);
#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"OS: {OS}\n", true);
                }

                // 以枚举形式返回平台 ID
                Platform = OS.Platform;

#if DEBUG
                Console.WriteLine("Platform: " + OS.Platform);
#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"Platform: {OS.Platform}\n", true);
                }

                // 当前安装的服务包
                ServicePack = OS.ServicePack;

#if DEBUG
                Console.WriteLine("ServicePack: " + OS.ServicePack);

#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"ServicePack: {OS.ServicePack}\n", true);
                }

                // 检索当前 CLR 版本
                ClrVersion = Environment.Version;

#if DEBUG
                Console.WriteLine("CLR version: " + ClrVersion);
#endif

                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"CLR version: {ClrVersion}\n", true);
                }

                // 操作系统的 toString 版本
                VersionString = OS.VersionString;

#if DEBUG
                Console.WriteLine("VersionString: " + OS.VersionString);
#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"VersionString: {OS.VersionString}\n", true);
                }

                // 我的电脑上有 4 个处理器
                ProcessorCount = Environment.ProcessorCount;

#if DEBUG
                Console.WriteLine("processorCount: " + ProcessorCount);
#endif

                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"processorCount: {ProcessorCount}\n", true);
                }

                // 返回逻辑驱动器列表：例如 C: 和 D:
                LogicalDrives = Environment.GetLogicalDrives();

                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"Logical Drives:\n", true);
                    foreach (string key in LogicalDrives) {
                        Tools.SaveData(Globals.Output_Filename, $"{key} |\t", true);
                    }
                    Tools.SaveData(Globals.Output_Filename, $"\n", true);
                }

                // 如何查找系统的所有环境变量并遍历它们
                EnvVars = Environment.GetEnvironmentVariables();

                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"Environment variables:\n", true);
                    foreach (string key in EnvVars.Keys) {
                        Debug.WriteLine($"key: {key}: {EnvVars[key]}");

                        Tools.SaveData(Globals.Output_Filename, $"\t{key}: {EnvVars[key]}\n", true);
                    }
                }

                return 0;
            } catch (Exception ex) {
#if DEBUG
                Console.WriteLine($"Exception Message: {ex.Message}");
#endif
                return -1;
            }
        }
    }
}