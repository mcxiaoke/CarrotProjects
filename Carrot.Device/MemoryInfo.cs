using System;
using System.Collections.Generic;
using System.Text;

namespace Carrot.Device {

    /// <summary>
    /// \class MemoryInfo
    /// 提供计算机上安装的总体内存的概述。
    /// <seealso cref="MemoryBankInfo">
    /// </summary>
    public class MemoryInfo {

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public MemoryInfo() {
        }

        /// <summary>
        /// 计算机内存条数量
        /// </summary>
        public int NoOfMemoryBanks { get; set; }

        /// <summary>
        /// 内存条数据宽度
        /// </summary>
        public int DataWidth { get; set; }

        /// <summary>
        /// 内存总大小（字节）
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// 内存总大小（兆字节）
        /// </summary>
        public long TotalSizeMegaBytes { get; set; }

        /// <summary>
        /// 使用 MemoryBankInfo 实例列表提供的信息来设置 MemoryInfo 类的属性
        /// </summary>
        /// <param name="banks">内存条信息实例列表 (List<MemoryBankInfo>)</param>
        /// <returns>成功返回 0，异常返回 -1，输入为 null 返回 -2</returns>
        public int GetMemoryInfo(List<MemoryBankInfo> banks) {
            try {
                if (banks == null)
                    return -2;

                this.NoOfMemoryBanks = banks.Count;
                if (banks.Count > 0)
                {
                    this.DataWidth = banks[0].DataWidth;
                }

                long size = 0;
                foreach (var item in banks) {
                    size += item.Capacity;
                }

                this.TotalSize = size;
                this.TotalSizeMegaBytes = size / (1024 * 1024);
#if DEBUG
                Console.WriteLine($"Size (bytes): {size}\nSize (Mb): {size / (1024 * 1024)}");
#endif

                return 0;
            } catch (Exception ex) {
#if DEBUG
                Console.WriteLine($"Exception Message: {ex.Message}");
#endif
                return -1;
            }
        }

        /// <summary>
        /// 将 MemoryInfo 类的属性转换为字符串。
        /// </summary>
        /// <returns>string</returns>
        public override string ToString() {
            var str = new StringBuilder();

            str.AppendLine($"Number of Memory Banks: {NoOfMemoryBanks}");
            str.AppendLine($"Memory Data Width: {DataWidth}");
            str.AppendLine($"Memory Total Size (Bytes): {TotalSize}");
            str.AppendLine($"Memory Total Size (MegaBytes): {TotalSizeMegaBytes}");

            return str.ToString();
        }
    }
}