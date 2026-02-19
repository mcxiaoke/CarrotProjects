using System;
using System.Collections.Generic;
using System.Management;

namespace Carrot.Device {

    /// <summary>
    /// \class Explorer
    /// 一个实例化并调用所有负责发现已安装硬件/软件的类的类。
    /// </summary>
    public class Explorer {

        /// <summary>
        /// 默认构造函数，初始化所有硬件对象
        /// </summary>
        public Explorer() {
            System_CPUs = new List<CPUInfo>();
            System_Memory = new List<MemoryBankInfo>();
            System_VideoControllers = new List<VideoControllerInfo>();
            PlatformInformation = new PlatformInfo();
            MemoryInformation = new MemoryInfo();
            System_DiskDrives = new List<DiskDriveInfo>();
            System_DiskPartitions = new List<DiskPartition>();
        }

        /// <summary>
        /// 平台信息
        /// </summary>
        public PlatformInfo PlatformInformation { get; set; }

        /// <summary>
        /// 系统 CPU 列表
        /// </summary>
        public List<CPUInfo> System_CPUs { get; set; }

        /// <summary>
        /// 内存条信息列表
        /// </summary>
        public List<MemoryBankInfo> System_Memory { get; set; }

        /// <summary>
        /// 已安装内存的常规信息
        /// </summary>
        public MemoryInfo MemoryInformation { get; set; }

        /// <summary>
        /// 视频控制器信息列表
        /// </summary>
        public List<VideoControllerInfo> System_VideoControllers { get; set; }

        /// <summary>
        /// 磁盘驱动器信息列表
        /// </summary>
        public List<DiskDriveInfo> System_DiskDrives { get; set; }

        /// <summary>
        /// 磁盘分区信息列表
        /// </summary>
        public List<DiskPartition> System_DiskPartitions { get; set; }

        /// <summary>
        /// 此函数查询不同的软件/硬件记录以获取其属性
        /// </summary>
        /// <returns>如果成功则为 0，如果出现异常则为 -1</returns>
        public int Run() {
            try {
                int error = 0;

                // 收集通用系统信息
                PlatformInformation.GetSystemInfo();

                // 获取特定 CPU 信息
                GetCPUInfo();

                // 获取内存信息
                GetMemoryInfo();

                // 计算内存参数
                MemoryInformation.GetMemoryInfo(System_Memory);
#if DEBUG
                Console.WriteLine(MemoryInformation.ToString());
#endif

                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"{MemoryInformation}\n", true);
                }

                // 获取视频控制器信息
                GetVideoControllerInfo();

                // 获取磁盘信息
                GetDiskDriveInfo();

                // 获取磁盘分区
                GetDiskPartitionInfo();

                return error;
            } catch (Exception ex) {
#if DEBUG
                Console.WriteLine($"Exception Message: {ex.Message}");
#endif
                return -1;
            }
        }

        /// <summary>
        /// 查询 Win32_Processor 以提取所有已安装 CPU 的属性
        /// <seealso cref="CPUInfo"/>
        /// </summary>
        public void GetCPUInfo() {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            var objCol = searcher.Get();

#if DEBUG
            Console.WriteLine($"\n********** Processor Info **********");
            if (objCol != null) {
                Console.WriteLine($"Detected CPUs: {objCol.Count}");
            }
#endif

            if (Globals.Enable_File_Output) {
                Tools.SaveData(Globals.Output_Filename, $"\n********** Processor Info **********\n", true);
                if (objCol != null) {
                    Tools.SaveData(Globals.Output_Filename, $"\nDetected CPUs: {objCol.Count}\n", true);
                }
            }

            foreach (var mgtObject in objCol) {
                var cpu = new CPUInfo();
                cpu.GetCpuInfo((ManagementObject)mgtObject);

                System_CPUs.Add(cpu);

#if DEBUG
                Console.WriteLine(cpu);
                Tools.SaveData(Globals.Output_Filename, $"{cpu}\n", true);
#endif
            }
        }

        /// <summary>
        /// 查询 Win32_PhysicalMemory 以提取所有已安装内存条的属性
        /// <seealso cref="MemoryBankInfo"/>
        /// </summary>
        public void GetMemoryInfo() {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            var objCol = searcher.Get();

#if DEBUG
            Console.WriteLine($"\n********** Memory Info **********");
            if (objCol != null) {
                Console.WriteLine($"Detected memory banks: {objCol.Count}");
            }
#endif

            if (Globals.Enable_File_Output) {
                Tools.SaveData(Globals.Output_Filename, $"\n********** Memory Info **********\n", true);
                if (objCol != null) {
                    Tools.SaveData(Globals.Output_Filename, $"\nDetected memory banks: {objCol.Count}\n", true);
                }
            }

            foreach (var mgtObject in objCol) {
                var mem = new MemoryBankInfo();
                mem.GetMemInfo((ManagementObject)mgtObject);

                System_Memory.Add(mem);
#if DEBUG
                Console.WriteLine(mem);
#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"{mem}\n", true);
                }
            }
        }

        /// <summary>
        /// 查询 Win32_VideoController 以提取所有已安装视频控制器的属性
        /// <seealso cref="VideoControllerInfo"/>
        /// </summary>
        public void GetVideoControllerInfo() {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            var objCol = searcher.Get();

#if DEBUG
            Console.WriteLine($"\n********** Video Controllers **********");
            if (objCol != null) {
                Console.WriteLine($"\nDetected video controllers: {objCol.Count}");
            }
#endif

            if (Globals.Enable_File_Output) {
                Tools.SaveData(Globals.Output_Filename, $"\n********** Video Controllers **********\n", true);
                if (objCol != null) {
                    Tools.SaveData(Globals.Output_Filename, $"\nDetected video controllers: {objCol.Count}\n", true);
                }
            }

            foreach (var mgtObject in objCol) {
                var vid = new VideoControllerInfo();
                vid.GetVideoControllerInfo((ManagementObject)mgtObject);

                System_VideoControllers.Add(vid);

#if DEBUG
                Console.WriteLine(vid);
#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"{vid}\n", true);
                }
            }
        }

        /// <summary>
        /// 查询 Win32_DiskDrive 以提取所有已安装磁盘驱动器的属性
        /// <seealso cref="DiskDriveInfo"/>
        /// </summary>
        public void GetDiskDriveInfo() {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            var objCol = searcher.Get();

#if DEBUG
            Console.WriteLine($"\n********** Disk Drives **********");
            if (objCol != null) {
                Console.WriteLine($"Detected disk drives: {objCol.Count}");
            }
#endif

            if (Globals.Enable_File_Output) {
                Tools.SaveData(Globals.Output_Filename, $"\n********** Disk Drives **********\n", true);
                if (objCol != null) {
                    Tools.SaveData(Globals.Output_Filename, $"\nDetected disk drives: {objCol.Count}\n", true);
                }
            }

            foreach (var mgtObject in objCol) {
                var disk = new DiskDriveInfo();
                disk.GetDiskDriveInfo((ManagementObject)mgtObject);

                System_DiskDrives.Add(disk);

#if DEBUG
                Console.WriteLine(disk);
                Tools.SaveData(Globals.Output_Filename, $"{disk}\n", true);
#endif
            }
        }

        /// <summary>
        /// 查询 Win32_DiskPartition 以提取所有可用磁盘分区的属性
        /// <seealso cref="DiskPartition"/>
        /// </summary>
        public void GetDiskPartitionInfo() {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskPartition");
            var objCol = searcher.Get();

#if DEBUG
            Console.WriteLine($"\n********** Disk Partitions **********");
            if (objCol != null) {
                Console.WriteLine($"Detected partitions: {objCol.Count}");
            }
#endif
            if (Globals.Enable_File_Output) {
                Tools.SaveData(Globals.Output_Filename, $"\n********** Disk Partitions **********\n", true);
                if (objCol != null) {
                    Tools.SaveData(Globals.Output_Filename, $"\nDetected partitions: {objCol.Count}\n", true);
                }
            }

            foreach (var mgtObject in objCol) {
                var partition = new DiskPartition();
                partition.GetDiskPartitionInfo((ManagementObject)mgtObject);

                System_DiskPartitions.Add(partition);

#if DEBUG
                Console.WriteLine(partition);
#endif
                if (Globals.Enable_File_Output) {
                    Tools.SaveData(Globals.Output_Filename, $"{partition}\n", true);
                }
            }
        }
    }
}