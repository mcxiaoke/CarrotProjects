using System;
using System.Diagnostics;
using System.Management;
using System.Text;

namespace Carrot.Device {

    /// <summary>
    /// \class QuickSystemStats
    /// 一个快捷类，提供系统的一些实时统计信息（例如，CPU 使用率，空闲内存大小等）。
    /// </summary>
    public class QuickSystemStats {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public QuickSystemStats() {
            TotalMemSize = GetTotalRAMSize();
        }

        /// <summary>
        /// CPU 使用率百分比
        /// </summary>
        public double CpuUsage { get; private set; }

        /// <summary>
        /// 运行线程数
        /// </summary>
        public int ThreadCount { get; private set; }

        /// <summary>
        /// 空闲内存大小
        /// </summary>
        public double FreeMemSize { get; private set; }

        /// <summary>
        /// 已用内存百分比
        /// </summary>
        public double MemUsagePercent { get; private set; }

        /// <summary>
        /// 总内存大小
        /// </summary>
        public double TotalMemSize { get; private set; }

        /// <summary>
        /// 上下文切换次数
        /// </summary>
        public int CPUContextSwitches { get; private set; }

        /// <summary>
        /// 句柄数
        /// </summary>
        public int HandlesCount { get; private set; }

        /// <summary>
        /// CPU 每秒服务的系统调用数
        /// </summary>
        public int SystemCallsCount { get; private set; }

        /// <summary>
        /// 从磁盘读取的总字节数
        /// </summary>
        public int BytesReadFromDisk { get; private set; }

        /// <summary>
        /// 写入磁盘的总字节数
        /// </summary>
        public int BytesWrittenToDisk { get; private set; }

        /// <summary>
        /// 磁盘读取操作的平均时间
        /// </summary>
        public double AvgTimeDiskReadPerSeond { get; private set; }

        /// <summary>
        /// 磁盘写入操作的平均时间
        /// </summary>
        public double AvgTimeDiskWritePerSeond { get; private set; }

        /// <summary>
        /// 此函数获取 CPU 使用率、空闲内存大小和内存使用百分比
        /// </summary>
        public int GetStats() {
            try {
                FreeMemSize = Convert.ToDouble(GetFreeMemorySize());

                CpuUsage = Convert.ToDouble(GetCPUUsage());

                MemUsagePercent = (TotalMemSize > 0) ? (TotalMemSize - FreeMemSize) * 100.0 / TotalMemSize : 0.0;

                ThreadCount = Convert.ToInt32(GetThreadCount());

                CPUContextSwitches = Convert.ToInt32(GetContextSwitchesCount());

                HandlesCount = Convert.ToInt32(GetHandlesCount());

                SystemCallsCount = Convert.ToInt32(GetSystemCallsCount());

                BytesReadFromDisk = Convert.ToInt32(GetDiskReadBytesCount());

                BytesWrittenToDisk = Convert.ToInt32(GetDiskWriteBytesCount());

                AvgTimeDiskReadPerSeond = Convert.ToDouble(GetDiskReadPerSecond());

                AvgTimeDiskWritePerSeond = Convert.ToDouble(GetDiskWritePerSecond());

                return 0;
            } catch (Exception ex) {
                Console.WriteLine($"Exception: {ex.Message}");
                return -1;
            }
        }

        /// <summary>
        /// 构建包含类成员的字符串
        /// </summary>
        /// <returns>String</returns>
        public override string ToString() {
            var oStr = new StringBuilder();

            oStr.AppendLine($"Total memory size: {TotalMemSize} Mbytes");
            oStr.AppendLine($"Free memory: {FreeMemSize} Mbytes");
            oStr.AppendLine($"Memory usage: {MemUsagePercent}%");
            oStr.AppendLine($"CPU usage: {CpuUsage}%");
            oStr.AppendLine($"Context switches: {CPUContextSwitches}");
            oStr.AppendLine($"No. threads: {ThreadCount}");
            oStr.AppendLine($"No. handles: {HandlesCount}");
            oStr.AppendLine($"System calls: {SystemCallsCount}");
            oStr.AppendLine($"Bytes read from the disk: {BytesReadFromDisk} bytes");
            oStr.AppendLine($"Bytes written to the disk: {BytesWrittenToDisk} bytes");
            oStr.AppendLine($"Avg. disk reading time: {AvgTimeDiskReadPerSeond}s");
            oStr.AppendLine($"Avg. disk writing time: {AvgTimeDiskWritePerSeond}s");

            return oStr.ToString();
        }

        /// <summary>
        /// 获取 RAM 内存的总大小
        /// </summary>
        /// <returns>Double value</returns>
        private double GetTotalRAMSize() {
            using var mc = new ManagementClass("Win32_ComputerSystem");
            var moc = mc.GetInstances();
            double size = 0;

            foreach (var item in moc) {
                size += Math.Round(Convert.ToDouble(item.Properties["TotalPhysicalMemory"].Value) / 1048576, 0);
            }

            return size;
        }

        /// <summary>
        /// 获取当前 CPU 使用时间
        /// </summary>
        /// <returns>Object</returns>
        private object GetCPUUsage() {
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            // will always start at 0
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(100);

            // now matches task manager reading
            return cpuCounter.NextValue();
        }

        /// <summary>
        /// 报告空闲内存的总大小
        /// </summary>
        /// <returns>Object</returns>
        private object GetFreeMemorySize() {
            using var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            return ramCounter.NextValue();
        }

        /// <summary>
        /// 报告 CPU 上运行的线程总数
        /// </summary>
        /// <returns>Object</returns>
        private object GetThreadCount() {
            using var threadCount = new PerformanceCounter("Process", "Thread Count", "_Total");
            return threadCount.NextValue();
        }

        /// <summary>
        /// 获取每秒的上下文切换（线程更改）总数
        /// </summary>
        /// <returns>Object</returns>
        private object GetContextSwitchesCount() {
            using var contextSwitchesCount = new PerformanceCounter("System", "Context Switches/sec", null);
            return contextSwitchesCount.NextValue();
        }

        /// <summary>
        /// 报告进程为其创建的对象打开的句柄数
        /// </summary>
        /// <returns>Object</returns>
        private object GetHandlesCount() {
            using var threadHandlesCount = new PerformanceCounter("Process", "Handle Count", "_Total");
            return threadHandlesCount.NextValue();
        }

        /// <summary>
        /// 报告 CPU 每秒服务的系统调用数
        /// </summary>
        /// <returns>Object</returns>
        private object GetSystemCallsCount() {
            using var systemCallsCount = new PerformanceCounter("System", "System Calls/sec", null);
            return systemCallsCount.NextValue();
        }

        /// <summary>
        /// 报告从磁盘每秒读取的总字节数
        /// </summary>
        /// <returns>Object</returns>
        private object GetDiskReadBytesCount() {
            using var diskReadBytesCount = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", "_Total");
            return diskReadBytesCount.NextValue();
        }

        /// <summary>
        /// 报告每秒写入磁盘的总字节数
        /// </summary>
        /// <returns>Object</returns>
        private object GetDiskWriteBytesCount() {
            using var diskWriteBytesCount = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", "_Total");
            return diskWriteBytesCount.NextValue();
        }

        /// <summary>
        /// 报告从磁盘读取操作的平均时间
        /// </summary>
        /// <returns>Object</returns>
        private object GetDiskReadPerSecond() {
            using var diskReadPerSeond = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", "_Total");
            return diskReadPerSeond.NextValue();
        }

        /// <summary>
        /// 报告写入磁盘操作的平均时间
        /// </summary>
        /// <returns>Object</returns>
        private object GetDiskWritePerSecond() {
            using var diskWritePerSeond = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", "_Total");
            return diskWritePerSeond.NextValue();
        }
    }
}