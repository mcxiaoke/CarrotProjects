using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Carrot.Common;

namespace GenshinNotifier {

    /// <summary>
    /// 快捷方式辅助类
    /// 用于创建桌面快捷方式和管理开机自启动快捷方式
    /// </summary>
    internal static class ShortcutHelper {

        private static string StartupPath => Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        private static string ProgramPath => AppInfo.ExecutablePath ?? "";
        private static string ProgramName => Application.ProductName ?? "";
        private static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>
        /// 启用或禁用开机自启动
        /// </summary>
        /// <param name="enable">true=启用, false=禁用</param>
        public static void EnableAutoStart(bool enable = true) {
            Logger.Info($"EnableAutoStart {enable}");
            if (enable) {
                // 检查是否已存在快捷方式
                List<string> shortcuts = GetExistsShortcuts(StartupPath, ProgramPath);
                if (shortcuts.Count >= 2) {
                    // 如果存在多个，清理多余的
                    for (int i = 1; i < shortcuts.Count; i++) {
                        DeleteFile(shortcuts[i]);
                    }
                } else if (shortcuts.Count < 1) {
                    // 如果不存在，创建新的快捷方式 (最小化启动)
                    CreateShortcut(StartupPath, ProgramName, ProgramPath, "--autostart", 7);
                }
            } else {
                // 如果禁用，删除所有相关快捷方式
                GetExistsShortcuts(StartupPath, ProgramPath).ForEach(it => DeleteFile(it));
            }
        }

        /// <summary>
        /// 创建当前程序的桌面快捷方式
        /// </summary>
        public static bool CreateDesktopShortcut() {
            return CreateDesktopShortcut(ProgramName, ProgramPath);
        }

        /// <summary>
        /// 创建指定目标的桌面快捷方式
        /// </summary>
        public static bool CreateDesktopShortcut(string shortcutName, string targetPath) {
            Logger.Debug($"CreateDesktopShortcut {shortcutName} for {targetPath}");
            List<string> shortcutPaths = GetExistsShortcuts(DesktopPath, targetPath);
            if (shortcutPaths.Count == 0) {
                return CreateShortcut(DesktopPath, shortcutName, targetPath);
            }
            return true;
        }

        /// <summary>
        /// 创建快捷方式 (.lnk 文件)
        /// 使用 WScript.Shell COM 接口
        /// </summary>
        /// <param name="directory">快捷方式存放目录</param>
        /// <param name="shortcutName">快捷方式名称 (不含 .lnk 后缀)</param>
        /// <param name="targetPath">目标程序路径</param>
        /// <param name="arguments">启动参数</param>
        /// <param name="windowStyle">窗口样式 (1=正常, 3=最大化, 7=最小化)</param>
        /// <param name="description">描述信息</param>
        /// <param name="iconLocation">图标位置 (默认为目标程序)</param>
        /// <returns>创建成功返回 true</returns>
        // https://docs.microsoft.com/zh-cn/previous-versions/windows/internet-explorer/ie-developer/windows-scripting
        public static bool CreateShortcut(
            string directory,
            string shortcutName,
            string targetPath,
            string arguments = "",
            int windowStyle = 1,
            string description = "",
            string iconLocation = "") {
            Logger.Debug($"CreateShortcut in {directory} for {targetPath}");
            try {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string shortcutPath = Path.Combine(directory, $"{shortcutName}.lnk");
                
                Type? type = Type.GetTypeFromProgID("WScript.Shell");
                if (type == null) return false;
                dynamic? shell = Activator.CreateInstance(type);
                if (shell == null) return false;
                dynamic shortcut = shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = targetPath;
                shortcut.Arguments = arguments;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                // 1 normal; 3 maximized; 7 minimized;
                shortcut.WindowStyle = windowStyle;
                shortcut.Description = description;
                shortcut.IconLocation = string.IsNullOrWhiteSpace(iconLocation) ? targetPath : iconLocation;
                shortcut.Save();
                return true;
            } catch (Exception ex) {
                Logger.Error("CreateShortcut", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取指定目录下指向特定目标的所有快捷方式路径
        /// </summary>
        public static List<string> GetExistsShortcuts(string directory, string targetPath) {
            return Directory.GetFiles(directory, "*.lnk")
                .Where(it => GetShortcutTargetPath(it) == targetPath)
                .ToList();
        }

        /// <summary>
        /// 获取快捷方式指向的目标路径
        /// </summary>
        public static string GetShortcutTargetPath(string shortcutPath) {
            Logger.Verbose($"GetShortcutTargetPath for {shortcutPath}");
            if (System.IO.File.Exists(shortcutPath)) {
                try {
                    Type? type = Type.GetTypeFromProgID("WScript.Shell");
                    if (type == null) return "";
                    dynamic? shell = Activator.CreateInstance(type);
                    if (shell == null) return "";
                    dynamic shortcut = shell.CreateShortcut(shortcutPath);
                    return shortcut.TargetPath;
                } catch { return ""; }
            } else {
                return "";
            }
        }

        private static void DeleteFile(string path) {
            Logger.Debug($"DeleteFile {path}");
            FileAttributes attr = System.IO.File.GetAttributes(path);
            if (attr == FileAttributes.Directory) {
                Directory.Delete(path, true);
            } else {
                System.IO.File.Delete(path);
            }
        }
    }
}