using System;
using System.IO;

namespace Carrot.Device {

    /// <summary>
    /// \class Tools
    /// 收集用于不同目的（例如数据存储）的静态辅助函数
    /// </summary>
    internal static class Tools {

        /// <summary>
        /// 将字符串保存到磁盘上的文件
        /// </summary>
        /// <param name="filename">目标文件名和路径 (string)</param>
        /// <param name="data">要保存的文本 (string)</param>
        /// <param name="append">指示是否应追加文件的标志 (Boolean)</param>
        public static void SaveData(string filename, string data, bool append = true) {
            try {
                if (append) {
                    File.AppendAllText(filename, data);
                } else {
                    File.WriteAllText(filename, data);
                }
            } catch (Exception ex) {
                Console.Error.WriteLine($"An error has occured!\nMessage: {ex.Message}\nStack Trace: {ex.StackTrace}\nSource: {ex.Source}");
            }
        }
    }
}