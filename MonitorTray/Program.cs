using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;

namespace MonitorControlTray
{
    static class Program
    {
        private static string LogPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log"); }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            EnsureLogDirectory();
            
            Log("程序启动");
            
            try
            {
                Application.Run(new TrayApplicationContext());
            }
            catch (Exception ex)
            {
                Log($"程序异常退出: {ex}");
                MessageBox.Show($"程序异常退出: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void EnsureLogDirectory()
        {
            string logDir = Path.GetDirectoryName(LogPath);
            if (logDir != null && !Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        public static void Log(string message)
        {
            try
            {
                using (var writer = new StreamWriter(LogPath, append: true))
                {
                    writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                }
            }
            catch
            {
                // 日志写入失败时静默处理，不影响主程序
            }
        }

        public static void ShowError(string message)
        {
            Log($"错误: {message}");
            MessageBox.Show(message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowError(string message, Exception ex)
        {
            Log($"错误: {message} - {ex}");
            MessageBox.Show($"{message}\n详细信息: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public class TrayApplicationContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private AppConfig config;
        private string currentMode = "Daily";
        private System.Windows.Forms.Timer timer;
        private TimeSetting lastAppliedSetting = null;

        public TrayApplicationContext()
        {
            LoadConfig();

            if (config == null)
            {
                Program.ShowError("配置加载失败");
                Environment.Exit(1);
            }

            trayIcon = new NotifyIcon()
            {
                Icon = LoadTrayIcon(),
                ContextMenuStrip = new ContextMenuStrip(),
                Visible = true,
                Text = "显示器亮度控制 - " + currentMode
            };

            BuildMenu();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 60000;
            timer.Tick += (s, e) => ApplySettings();
            timer.Start();

            ApplySettings(force: true);

            Program.Log("托盘应用初始化完成");
        }

        /// <summary>
        /// 加载托盘图标
        /// 优先级：assets/icon.ico > assets/icon.png > 系统默认图标
        /// </summary>
        private Icon LoadTrayIcon()
        {
            // 优先使用 ico 格式
            string iconIcoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "icon.ico");
            if (File.Exists(iconIcoPath))
            {
                try
                {
                    return new Icon(iconIcoPath);
                }
                catch (Exception ex)
                {
                    Program.Log($"加载 icon.ico 失败: {ex.Message}");
                }
            }

            // 尝试使用 png 格式
            string iconPngPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "icon.png");
            if (File.Exists(iconPngPath))
            {
                try
                {
                    using (var bmp = new Bitmap(iconPngPath))
                    {
                        return Icon.FromHandle(bmp.GetHicon());
                    }
                }
                catch (Exception ex)
                {
                    Program.Log($"加载 icon.png 失败: {ex.Message}");
                }
            }

            Program.Log("未找到自定义图标，使用系统默认图标");
            return SystemIcons.Application;
        }

        /// <summary>
        /// 查找 ddccli.exe
        /// 优先级：当前目录 > PATH 环境变量
        /// </summary>
        private string? FindDdccli()
        {
            string? ddccliPath = null;

            // 1. 首先检查配置的路径
            if (!string.IsNullOrWhiteSpace(config.DdccliPath))
            {
                string configuredPath = config.DdccliPath;
                
                // 如果是绝对路径
                if (Path.IsPathRooted(configuredPath))
                {
                    if (File.Exists(configuredPath))
                    {
                        Program.Log($"使用配置的绝对路径: {configuredPath}");
                        return configuredPath;
                    }
                }
                // 如果是相对路径，优先从当前目录查找
                else
                {
                    string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath);
                    if (File.Exists(localPath))
                    {
                        Program.Log($"使用配置的相对路径（当前目录）: {localPath}");
                        return localPath;
                    }
                }
            }

            // 2. 检查当前目录下的 ddccli.exe
            string localDdccli = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ddccli.exe");
            if (File.Exists(localDdccli))
            {
                Program.Log($"在当前目录找到 ddccli.exe: {localDdccli}");
                return localDdccli;
            }

            // 3. 从 PATH 环境变量中查找
            string? pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (string dir in pathEnv.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(dir)) continue;
                    
                    string ddccliInPath = Path.Combine(dir.Trim(), "ddccli.exe");
                    if (File.Exists(ddccliInPath))
                    {
                        Program.Log($"在 PATH 中找到 ddccli.exe: {ddccliInPath}");
                        return ddccliInPath;
                    }
                }
            }

            return ddccliPath;
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (!File.Exists(configPath))
            {
                string errorMsg = "找不到 config.json 配置文件";
                Program.ShowError(errorMsg);
                Environment.Exit(1);
            }

            try
            {
                string jsonContent = File.ReadAllText(configPath);
                config = JsonSerializer.Deserialize<AppConfig>(jsonContent);

                if (!ValidateConfig(config))
                {
                    Environment.Exit(1);
                }

                Program.Log("配置文件加载成功");
            }
            catch (JsonException ex)
            {
                string errorMsg = "配置文件格式错误";
                Program.ShowError(errorMsg, ex);
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                string errorMsg = "加载配置文件失败";
                Program.ShowError(errorMsg, ex);
                Environment.Exit(1);
            }
        }

        private bool ValidateConfig(AppConfig config)
        {
            if (config == null)
            {
                Program.ShowError("配置对象为空");
                return false;
            }

            List<string> errors = new List<string>();

            if (config.Modes == null || config.Modes.Count == 0)
            {
                errors.Add("Modes 配置不能为空");
            }
            else
            {
                foreach (var mode in config.Modes)
                {
                    if (string.IsNullOrWhiteSpace(mode.Key))
                    {
                        errors.Add("模式名称不能为空");
                        continue;
                    }

                    if (mode.Value == null || mode.Value.Count == 0)
                    {
                        errors.Add($"模式 '{mode.Key}' 的时间设置不能为空");
                        continue;
                    }

                    foreach (var setting in mode.Value)
                    {
                        if (setting == null)
                        {
                            errors.Add($"模式 '{mode.Key}' 中存在空的时间设置项");
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(setting.Time))
                        {
                            errors.Add($"模式 '{mode.Key}' 中存在空的时间设置");
                        }
                        else if (!TimeSpan.TryParse(setting.Time, out _))
                        {
                            errors.Add($"模式 '{mode.Key}' 中的时间格式错误: {setting.Time}");
                        }

                        if (setting.Brightness < 0 || setting.Brightness > 100)
                        {
                            errors.Add($"模式 '{mode.Key}' 中的亮度值超出范围 (0-100): {setting.Brightness}");
                        }

                        if (setting.Contrast < 0 || setting.Contrast > 100)
                        {
                            errors.Add($"模式 '{mode.Key}' 中的对比度值超出范围 (0-100): {setting.Contrast}");
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                string errorMsg = "配置验证失败:\n" + string.Join("\n", errors);
                Program.ShowError(errorMsg);
                return false;
            }

            return true;
        }

        private void BuildMenu()
        {
            if (trayIcon == null || config == null) return;

            trayIcon.ContextMenuStrip.Items.Clear();

            var modeMenuItem = new ToolStripMenuItem("切换模式");
            foreach (var mode in config.Modes.Keys)
            {
                var item = new ToolStripMenuItem(mode, null, (s, e) => SwitchMode(mode))
                {
                    Checked = (mode == currentMode)
                };
                modeMenuItem.DropDownItems.Add(item);
            }

            trayIcon.ContextMenuStrip.Items.Add(modeMenuItem);
            trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            trayIcon.ContextMenuStrip.Items.Add("手动刷新", null, (s, e) => ApplySettings(force: true));
            trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            
            // 自启动菜单
            var autoStartMenuItem = new ToolStripMenuItem("开机自启");
            bool isAutoStartEnabled = IsAutoStartEnabled();
            var enableItem = new ToolStripMenuItem("启用", null, (s, e) => SetAutoStart(true))
            {
                Checked = isAutoStartEnabled
            };
            var disableItem = new ToolStripMenuItem("禁用", null, (s, e) => SetAutoStart(false))
            {
                Checked = !isAutoStartEnabled
            };
            autoStartMenuItem.DropDownItems.Add(enableItem);
            autoStartMenuItem.DropDownItems.Add(disableItem);
            trayIcon.ContextMenuStrip.Items.Add(autoStartMenuItem);
            
            trayIcon.ContextMenuStrip.Items.Add("退出", null, (s, e) => Exit());
        }

        /// <summary>
        /// 检查是否已启用开机自启
        /// </summary>
        private bool IsAutoStartEnabled()
        {
            string shortcutPath = GetStartupShortcutPath();
            return File.Exists(shortcutPath);
        }

        /// <summary>
        /// 获取自启动快捷方式的路径
        /// </summary>
        private string GetStartupShortcutPath()
        {
            string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string appName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", "");
            return Path.Combine(startupFolder, $"{appName}.lnk");
        }

        /// <summary>
        /// 设置开机自启状态
        /// </summary>
        private void SetAutoStart(bool enable)
        {
            string shortcutPath = GetStartupShortcutPath();
            
            try
            {
                if (enable)
                {
                    // 创建快捷方式
                    string exePath = Application.ExecutablePath;
                    CreateShortcut(shortcutPath, exePath, "显示器亮度控制");
                    Program.Log($"已启用开机自启: {shortcutPath}");
                    trayIcon.ShowBalloonTip(2000, "提示", "已启用开机自启", ToolTipIcon.Info);
                }
                else
                {
                    // 删除快捷方式
                    if (File.Exists(shortcutPath))
                    {
                        File.Delete(shortcutPath);
                    }
                    Program.Log($"已禁用开机自启: {shortcutPath}");
                    trayIcon.ShowBalloonTip(2000, "提示", "已禁用开机自启", ToolTipIcon.Info);
                }
                
                // 刷新菜单
                BuildMenu();
            }
            catch (Exception ex)
            {
                string errorMsg = $"设置开机自启失败: {ex.Message}";
                Program.Log(errorMsg);
                trayIcon.ShowBalloonTip(3000, "错误", errorMsg, ToolTipIcon.Error);
            }
        }

        /// <summary>
        /// 创建快捷方式
        /// </summary>
        private void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            // 使用 COM 接口创建快捷方式
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType == null)
            {
                // 备选方案：直接复制当前 exe 的快捷方式（如果存在的话）
                // 或者使用 PowerShell 创建
                CreateShortcutViaPowerShell(shortcutPath, targetPath, description);
                return;
            }

            dynamic shell = Activator.CreateInstance(shellType);
            try
            {
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.Description = description;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.Save();
            }
            finally
            {
                Marshal.ReleaseComObject(shell);
            }
        }

        /// <summary>
        /// 通过 PowerShell 创建快捷方式（备选方案）
        /// </summary>
        private void CreateShortcutViaPowerShell(string shortcutPath, string targetPath, string description)
        {
            string psScript = $@"
                $WshShell = New-Object -ComObject WScript.Shell
                $Shortcut = $WshShell.CreateShortcut('{shortcutPath}')
                $Shortcut.TargetPath = '{targetPath}'
                $Shortcut.Description = '{description}'
                $Shortcut.WorkingDirectory = '{Path.GetDirectoryName(targetPath)}'
                $Shortcut.Save()
            ";
            
            var psi = new ProcessStartInfo
            {
                FileName = "powershell",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript.Replace("\"", "\\\"")}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (var process = Process.Start(psi))
            {
                process?.WaitForExit();
            }
        }

        private void SwitchMode(string newMode)
        {
            if (currentMode != newMode)
            {
                currentMode = newMode;
                if (trayIcon != null)
                {
                    trayIcon.Text = "显示器亮度控制 - " + currentMode;
                }
                BuildMenu();
                ApplySettings(force: true);
                Program.Log($"切换到 {newMode} 模式");
            }
        }

        private void ApplySettings(bool force = false)
        {
            if (config == null || !config.Modes.ContainsKey(currentMode))
            {
                Program.Log($"当前模式 '{currentMode}' 不存在于配置中");
                return;
            }

            TimeSpan currentTime = DateTime.Now.TimeOfDay;
            var timeSettings = config.Modes[currentMode]
                .OrderByDescending(s => TimeSpan.Parse(s.Time))
                .ToList();

            if (timeSettings.Count == 0)
            {
                Program.Log($"模式 '{currentMode}' 没有时间设置");
                return;
            }

            TimeSetting activeSetting = timeSettings.FirstOrDefault(s => currentTime >= TimeSpan.Parse(s.Time));
            if (activeSetting == null)
            {
                activeSetting = timeSettings.Last();
            }

            if (force || lastAppliedSetting == null ||
                lastAppliedSetting.Brightness != activeSetting.Brightness ||
                lastAppliedSetting.Contrast != activeSetting.Contrast)
            {
                lastAppliedSetting = activeSetting;
                ExecuteDdccli(activeSetting.Brightness, activeSetting.Contrast);
            }
        }

        private void ExecuteDdccli(int brightness, int contrast)
        {
            // 使用新的 FindDdccli 方法查找可执行文件
            string? ddccliPath = FindDdccli();
            
            if (string.IsNullOrEmpty(ddccliPath))
            {
                string errorMsg = "找不到 ddccli.exe（搜索了当前目录和 PATH）";
                Program.Log(errorMsg);
                trayIcon.ShowBalloonTip(3000, "错误", errorMsg, ToolTipIcon.Error);
                return;
            }

            try
            {
                using (var process = Process.Start(new ProcessStartInfo
                {
                    FileName = ddccliPath,
                    Arguments = $"-b {brightness} -c {contrast}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }))
                {
                    if (process != null)
                    {
                        process.WaitForExit();

                        if (process.ExitCode != 0)
                        {
                            string errorOutput = process.StandardError.ReadToEnd();
                            string errorMsg = $"ddccli 执行失败 (ExitCode: {process.ExitCode})";
                            if (!string.IsNullOrWhiteSpace(errorOutput))
                            {
                                errorMsg += $"\n错误信息: {errorOutput.Trim()}";
                            }
                            Program.Log(errorMsg);
                            trayIcon.ShowBalloonTip(3000, "警告", $"设置失败: 亮度 {brightness}, 对比度 {contrast}", ToolTipIcon.Warning);
                        }
                        else
                        {
                            Program.Log($"成功设置: 亮度 {brightness}, 对比度 {contrast}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMsg = $"执行 ddccli 时发生异常: {ex.Message}";
                Program.Log(errorMsg);
                trayIcon.ShowBalloonTip(3000, "错误", errorMsg, ToolTipIcon.Error);
            }
        }

        private void Exit()
        {
            trayIcon.Visible = false;
            Program.Log("程序退出");
            Application.Exit();
        }
    }

    public class AppConfig
    {
        public string? DdccliPath { get; set; }
        public Dictionary<string, List<TimeSetting>>? Modes { get; set; }
    }

    public class TimeSetting
    {
        public string? Time { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }
    }
}