using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using Carrot.Common;

namespace SharpUpdater {

    internal static class SharpUtils {
        public static string ExecutablePath => AppInfo.ExecutablePath;
        public static string ExecutableName => AppInfo.ModuleName;

        private static void TextBoxLog(RichTextBox box, string s, Color? c = null) {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;
            box.SelectionColor = c ?? box.ForeColor;
            box.AppendText((box.Lines.Length == 0 ? "" : Environment.NewLine) + DateTime.Now + "\t" + s);
            box.SelectionColor = box.ForeColor;
        }

        public static string SimpleRelativePath(string relativeTo, string path) {
            return Path.GetFullPath(path).Substring(Path.GetFullPath(relativeTo).Length);
        }

        public static void CheckOrCreateDirectory(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static string ZipFileFind(string zipSource, string fileName) {
            var zipPath = Path.GetFullPath(zipSource);
            using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                return archive.Entries.ToList().Find(it => it.FullName.EndsWith(fileName))?.FullName;
            }
        }

        public static bool ZipFileContains(string zipSource, string fileName) {
            var zipPath = Path.GetFullPath(zipSource);
            using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                return archive.Entries.Any(it => it.FullName.EndsWith(fileName));
            }
        }

        public static void UnzipFile(string zipSource,
            string zipDest,
            bool backupOld = false,
            bool stripPrefix = false, string prefixStr = null) {
            var zipPath = Path.GetFullPath(zipSource);
            var destPath = Path.GetFullPath(zipDest);
            if (!File.Exists(zipPath)) { return; }
            var backupPath = backupOld ? Path.Combine(destPath, "backups") : null;
            Logger.Debug($"UnzipFile \nSRC={zipPath} \nDST={destPath} \nbackup={backupPath} " +
                $"\nstrip={stripPrefix} prefix={prefixStr}");

            CheckOrCreateDirectory(destPath);
            if (!string.IsNullOrWhiteSpace(backupPath)) {
                CheckOrCreateDirectory(backupPath);
            }
            using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                var entries = archive.Entries;
                string prefix = string.Empty;
                if (stripPrefix) {
                    if (prefixStr != null && prefixStr.EndsWith("/")) {
                        prefix = prefixStr;
                    } else {
                        var fileNames = entries.Select(e => e.FullName);
                        //Logger.Debug(string.Join("\n", fileNames.ToArray()));
                        var prefixFound = GetCommonStringPrefix(fileNames);
                        if (prefixFound != null && prefixFound.EndsWith("/")) {
                            prefix = prefixFound;
                        }
                    }
                }
                Logger.Debug($"UnzipFile prefix={prefix}");
                var selfExePath = ExecutablePath;
                foreach (ZipArchiveEntry entry in entries) {
                    //Logger.Debug(entry.FullName);
                    if (entry.Length == 0) {
                        Logger.Debug("UnzipFile skip " + entry.FullName);
                        continue;
                    }
                    var fullName = entry.FullName;
                    //Logger.Debug($"fullName old={fullName}");
                    if (!string.IsNullOrWhiteSpace(prefix)) {
                        if (fullName.StartsWith(prefix)) {
                            fullName = fullName.Remove(0, prefix.Length);
                        }
                        //Logger.Debug($"fullName new={fullName}");
                    }
                    string entryDestination = Path.GetFullPath(Path.Combine(destPath, fullName));
                    Logger.Debug($"dest={entryDestination}");
                    if (File.Exists(entryDestination)) {
                        if (entryDestination == selfExePath) {
                            // current update is running, cannot replace
                            // so add pending suffix, replace on closed
                            entryDestination += ".pending";
                            if (File.Exists(entryDestination)) {
                                File.Delete(entryDestination);
                            }
                            Logger.Debug("UnzipFile pending " + entryDestination);
                        } else {
                            if (backupOld) {
                                var rp = SimpleRelativePath(destPath, entryDestination);
                                var destinationBackupPath = Path.Combine(backupPath, rp);
                                if (File.Exists(destinationBackupPath)) {
                                    File.Delete(destinationBackupPath);
                                }
                                var destinationBackupDir = Path.GetDirectoryName(destinationBackupPath);
                                if (!Directory.Exists(destinationBackupDir)) {
                                    Directory.CreateDirectory(destinationBackupDir);
                                }
                                Logger.Debug($"backup={destinationBackupPath}");
                                File.Copy(entryDestination, destinationBackupPath);
                            } else {
                                File.Delete(entryDestination);
                            }
                        }
                    }
                    Logger.Debug("UnzipFile ==> " + entryDestination);
                    FileInfo fileInfo = new FileInfo(entryDestination);
                    fileInfo.Directory.Create();
                    entry.ExtractToFile(entryDestination, true);
                }
            }
            Logger.Debug($"UnzipFile done.");
        }

        public static string FormatFileSize(long lSize) {
            double size = lSize;
            int index = 0;
            for (; size > 1024; index++)
                size /= 1024;
            return size.ToString("0.00 " + new[] { "B", "KB", "MB", "GB", "TB" }[index]);
        }

        // slow
        public static string GetCommonStringPrefix2(IEnumerable<string> strings) {
            var commonPrefix = strings.FirstOrDefault() ?? "";
            foreach (var s in strings) {
                var potentialMatchLength = Math.Min(s.Length, commonPrefix.Length);

                if (potentialMatchLength < commonPrefix.Length)
                    commonPrefix = commonPrefix.Substring(0, potentialMatchLength);

                for (var i = 0; i < potentialMatchLength; i++) {
                    if (s[i] != commonPrefix[i]) {
                        commonPrefix = commonPrefix.Substring(0, i);
                        break;
                    }
                }
            }
            return commonPrefix;
        }

        // https://stackoverflow.com/questions/2070356 fast
        public static string GetCommonStringPrefix(IEnumerable<string> strings) {
            var keys = strings.ToArray();
            Array.Sort(keys, StringComparer.InvariantCulture);
            string a1 = keys[0], a2 = keys[keys.Length - 1];
            int L = a1.Length, i = 0;
            while (i < L && a1[i] == a2[i]) {
                i++;
            }
            return a1.Substring(0, i);
        }

        public static List<string> GetFilesInFolder(string path) {
            return Directory.GetFiles(path).Select(it => Path.GetFileName(it)).ToList();
        }

        public static Exception StopProcessByPath(string fullpath) {
            Logger.Debug($"StopProcessByPath fullpath={fullpath}");
            var fileName = Path.GetFileName(fullpath);
            var moduleName = Path.GetFileNameWithoutExtension(fileName);
            try {
                Process[] existing = Process.GetProcessesByName(moduleName);
                foreach (Process p in existing) {
                    Logger.Debug($"StopProcessByName process={p.ProcessName} {p.Id} {p.MainModule.FileName} {p.MainModule.ModuleName}");
                    string path = p.MainModule.FileName;
                    if (path == fullpath) {
                        p.Kill();
                        p.WaitForExit(100);
                    }
                }
                return null;
            } catch (Exception ex) {
                Logger.Debug($"StopProcessByPath error={ex}");
                return ex;
            }
        }
    }
}