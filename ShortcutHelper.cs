using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IWshRuntimeLibrary;

namespace GenshinNotifier {
    public class ShortcutHelper {
        public static string ProgramFileName => Process.GetCurrentProcess().MainModule.FileName;
        public static string ProgramModuleName => Process.GetCurrentProcess().MainModule.ModuleName;

        private static string StartupPath => Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        private static string ProgramPath => Process.GetCurrentProcess().MainModule.FileName;
        private static string ProgramName => Application.ProductName;
        private static string DesktopPath => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        public static void EnableAutoStart(bool enable = true) {
            Logger.Info($"EnableAutoStart {enable}");
            if (enable) {
                List<string> shortcuts = GetExistsShortcuts(StartupPath, ProgramPath);
                if (shortcuts.Count >= 2) {
                    for (int i = 1; i < shortcuts.Count; i++) {
                        DeleteFile(shortcuts[i]);
                    }
                } else if (shortcuts.Count < 1) {
                    var targetPath = $"{ProgramPath} autostart";
                    CreateShortcut(StartupPath, ProgramName, targetPath, 7);
                }
            } else {
                GetExistsShortcuts(StartupPath, ProgramPath).ForEach(it => DeleteFile(it));
            }
        }

        public static bool CreateDesktopShortcut() {
            return CreateDesktopShortcut(ProgramName, ProgramPath);
        }

        public static bool CreateDesktopShortcut(string shortcutName, string targetPath) {
            Logger.Debug($"CreateDesktopShortcut {shortcutName} for {targetPath}");
            List<string> shortcutPaths = GetExistsShortcuts(DesktopPath, targetPath);
            if (shortcutPaths.Count == 0) {
                return CreateShortcut(DesktopPath, shortcutName, targetPath);
            }
            return true;
        }

        // https://docs.microsoft.com/zh-cn/previous-versions/windows/internet-explorer/ie-developer/windows-scripting
        public static bool CreateShortcut(
            string directory,
            string shortcutName,
            string targetPath,
            int windowStyle = 1,
            string description = null,
            string iconLocation = null) {
            Logger.Debug($"CreateShortcut in {directory} for {targetPath}");
            try {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string shortcutPath = Path.Combine(directory, $"{shortcutName}.lnk");
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
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

        public static List<string> GetExistsShortcuts(string directory, string targetPath) {
            return Directory.GetFiles(directory, "*.lnk")
                .Where(it => GetShortcutTargetPath(it) == targetPath)
                .ToList();
        }

        public static string GetShortcutTargetPath(string shortcutPath) {
            Logger.Verbose($"GetShortcutTargetPath for {shortcutPath}");
            if (System.IO.File.Exists(shortcutPath)) {
                WshShell shell = new WshShell();
                IWshShortcut shortct = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                return shortct.TargetPath;
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
