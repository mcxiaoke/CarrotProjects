using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace MonitorControl
{
    class Program
    {
        static void Main(string[] args)
        {
            // 默认模式设为 Daily，或者通过命令行参数传入（例如：MonitorControl.exe Game）
            string mode = args.Length > 0 ? args[0] : "Daily";

            // 读取配置文件
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"错误：找不到配置文件 {configPath}");
                return;
            }

            string jsonContent = File.ReadAllText(configPath);
            AppConfig config = JsonSerializer.Deserialize<AppConfig>(jsonContent);

            if (!config.Modes.ContainsKey(mode))
            {
                Console.WriteLine($"错误：在配置文件中找不到模式 '{mode}'");
                return;
            }

            // 获取当前时间
            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            Console.WriteLine($"当前时间: {currentTime:hh\\:mm}, 正在应用模式: {mode}");

            // 获取该模式下的所有时间配置，并按时间从晚到早排序
            var timeSettings = config.Modes[mode]
                .OrderByDescending(s => TimeSpan.Parse(s.Time))
                .ToList();

            TimeSetting activeSetting = null;

            // 查找适用的时间段
            foreach (var setting in timeSettings)
            {
                if (currentTime >= TimeSpan.Parse(setting.Time))
                {
                    activeSetting = setting;
                    break;
                }
            }

            // 如果当前时间早于当天的第一个设定时间（例如凌晨 1 点），则应用前一天最晚的设定
            if (activeSetting == null)
            {
                activeSetting = timeSettings.First(); 
            }

            Console.WriteLine($"设定的亮度 (b): {activeSetting.Brightness}, 对比度 (c): {activeSetting.Contrast}");

            // 执行 ddccli.exe
            ExecuteDdccli(config.DdccliPath, activeSetting.Brightness, activeSetting.Contrast);
        }

        static void ExecuteDdccli(string executablePath, int brightness, int contrast)
        {
            // 如果 executablePath 不是绝对路径，则默认它与当前程序在同一目录
            if (!Path.IsPathRooted(executablePath))
            {
                executablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, executablePath);
            }

            string arguments = $"-b {brightness} -c {contrast}";

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true // 隐藏黑框
                };

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
                Console.WriteLine("显示器设置已更新。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行 {executablePath} 失败: {ex.Message}");
            }
        }
    }

    // --- 数据模型 ---
    public class AppConfig
    {
        public string DdccliPath { get; set; }
        public Dictionary<string, List<TimeSetting>> Modes { get; set; }
    }

    public class TimeSetting
    {
        public string Time { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }
    }
}