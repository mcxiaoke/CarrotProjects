using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Carrot.Common;

namespace SharpUpdater {

    /// <summary>
    /// 更新备份管理器
    /// </summary>
    public class UpdateBackupManager {
        private readonly string _backupRoot;
        private readonly string _appBase;
        private readonly string _backupManifestFile;

        public UpdateBackupManager(string appBase) {
            _appBase = Path.GetFullPath(appBase);
            _backupRoot = Path.Combine(_appBase, ".update_backups");
            _backupManifestFile = Path.Combine(_backupRoot, "manifest.json");
            Directory.CreateDirectory(_backupRoot);
        }

        /// <summary>
        /// 创建更新前的备份
        /// </summary>
        /// <param name="filesToBackup">需要备份的文件列表</param>
        /// <returns>备份ID</returns>
        public string CreateBackup(List<string> filesToBackup) {
            var backupId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupDir = Path.Combine(_backupRoot, backupId);

            try {
                Directory.CreateDirectory(backupDir);
                Logger.Debug($"CreateBackup: 创建备份目录 {backupDir}");

                var manifest = new BackupManifest {
                    BackupId = backupId,
                    CreatedAt = DateTime.Now,
                    Files = new List<BackupFileInfo>()
                };

                foreach (var relativePath in filesToBackup) {
                    var sourceFile = Path.Combine(_appBase, relativePath);
                    if (File.Exists(sourceFile)) {
                        var destFile = Path.Combine(backupDir, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                        File.Copy(sourceFile, destFile, true);

                        var fileInfo = new FileInfo(sourceFile);
                        manifest.Files.Add(new BackupFileInfo {
                            RelativePath = relativePath,
                            Size = fileInfo.Length,
                            LastWriteTime = fileInfo.LastWriteTime
                        });

                        Logger.Debug($"CreateBackup: 备份文件 {relativePath}");
                    }
                }

                // 保存备份清单
                var manifestJson = Newtonsoft.Json.JsonConvert.SerializeObject(manifest, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_backupManifestFile, manifestJson, Encoding.UTF8);

                Logger.Debug($"CreateBackup: 备份完成，ID={backupId}，文件数={manifest.Files.Count}");
                return backupId;
            } catch (Exception ex) {
                Logger.Error($"CreateBackup 失败: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// 从备份恢复
        /// </summary>
        /// <param name="backupId">备份ID（为空则使用最新备份）</param>
        /// <returns>是否恢复成功</returns>
        public bool RestoreBackup(string? backupId = null) {
            try {
                if (backupId == null) {
                    backupId = GetLatestBackupId();
                    if (backupId == null) {
                        Logger.Debug("RestoreBackup: 没有找到可用的备份");
                        return false;
                    }
                }

                var backupDir = Path.Combine(_backupRoot, backupId);
                if (!Directory.Exists(backupDir)) {
                    Logger.Debug($"RestoreBackup: 备份目录不存在 {backupDir}");
                    return false;
                }

                var manifestFile = Path.Combine(backupDir, "manifest.json");
                if (!File.Exists(manifestFile)) {
                    Logger.Debug($"RestoreBackup: 备份清单不存在 {manifestFile}");
                    return false;
                }

                var manifestJson = File.ReadAllText(manifestFile, Encoding.UTF8);
                var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<BackupManifest>(manifestJson);

                if (manifest == null) {
                    Logger.Debug("RestoreBackup: 无法解析备份清单");
                    return false;
                }

                Logger.Debug($"RestoreBackup: 开始恢复备份 ID={backupId}");

                foreach (var fileInfo in manifest.Files) {
                    var sourceFile = Path.Combine(backupDir, fileInfo.RelativePath);
                    var destFile = Path.Combine(_appBase, fileInfo.RelativePath);

                    if (File.Exists(sourceFile)) {
                        Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                        File.Copy(sourceFile, destFile, true);
                        Logger.Debug($"RestoreBackup: 恢复文件 {fileInfo.RelativePath}");
                    }
                }

                Logger.Debug($"RestoreBackup: 恢复完成，文件数={manifest.Files.Count}");
                return true;
            } catch (Exception ex) {
                Logger.Error($"RestoreBackup 失败: {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取最新的备份ID
        /// </summary>
        public string? GetLatestBackupId() {
            if (!File.Exists(_backupManifestFile)) {
                return null;
            }

            try {
                var manifestJson = File.ReadAllText(_backupManifestFile, Encoding.UTF8);
                var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<BackupManifest>(manifestJson);
                return manifest?.BackupId;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// 清理旧备份（保留最近N个）
        /// </summary>
        public void CleanupOldBackups(int keepCount = 3) {
            try {
                var backupDirs = Directory.GetDirectories(_backupRoot)
                    .OrderByDescending(d => d)
                    .Skip(keepCount)
                    .ToList();

                foreach (var dir in backupDirs) {
                    try {
                        Directory.Delete(dir, true);
                        Logger.Debug($"CleanupOldBackups: 删除旧备份 {dir}");
                    } catch (Exception ex) {
                        Logger.Debug($"CleanupOldBackups: 删除失败 {ex.Message}");
                    }
                }
            } catch (Exception ex) {
                Logger.Error($"CleanupOldBackups 失败: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// 备份清单
    /// </summary>
    public class BackupManifest {
        public string BackupId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<BackupFileInfo> Files { get; set; } = new List<BackupFileInfo>();
    }

    /// <summary>
    /// 备份文件信息
    /// </summary>
    public class BackupFileInfo {
        public string RelativePath { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastWriteTime { get; set; }
    }

    internal static class SharpUtils {
        public static string ExecutablePath => AppInfo.ExecutablePath;
        public static string ExecutableName => AppInfo.ModuleName;

        public static string SimpleRelativePath(string relativeTo, string path) {
            return Path.GetFullPath(path).Substring(Path.GetFullPath(relativeTo).Length);
        }

        public static void CheckOrCreateDirectory(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public static string? ZipFileFind(string zipSource, string fileName) {
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
            bool stripPrefix = false, string? prefixStr = null) {
            var zipPath = Path.GetFullPath(zipSource);
            var destPath = Path.GetFullPath(zipDest);
            if (!File.Exists(zipPath)) { return; }
            var backupPath = backupOld ? Path.Combine(destPath, "backups") : string.Empty;
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
                    if (prefixStr != null && prefixStr.EndsWith('/')) {
                        prefix = prefixStr;
                    } else {
                        var fileNames = entries.Select(e => e.FullName);
                        //Logger.Debug(string.Join("\n", fileNames.ToArray()));
                        var prefixFound = GetCommonStringPrefix(fileNames);
                        if (prefixFound != null && prefixFound.EndsWith('/')) {
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
                                string destinationBackupDir = Path.GetDirectoryName(destinationBackupPath) ?? string.Empty;
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
                    fileInfo?.Directory?.Create();
                    entry.ExtractToFile(entryDestination, true);
                }
            }
            Logger.Debug($"UnzipFile done.");
        }

        private static readonly string[] SizeUnits = new[] { "B", "KB", "MB", "GB", "TB" };

        public static string FormatFileSize(long lSize) {
            double size = lSize;
            int index = 0;
            for (; size > 1024; index++)
                size /= 1024;
            return size.ToString("0.00 " + SizeUnits[index]);
        }

        /// <summary>
        /// 计算文件的 SHA256 哈希值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>小写的十六进制哈希字符串</returns>
        public static string ComputeFileSHA256(string filePath) {
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException($"文件不存在: {filePath}");
            }

            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hashBytes = sha256.ComputeHash(stream);
            return Convert.ToHexStringLower(hashBytes);
        }

        /// <summary>
        /// 验证文件的 SHA256 哈希值
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="expectedHash">预期的哈希值（忽略大小写）</param>
        /// <returns>验证是否通过</returns>
        public static bool VerifyFileSHA256(string filePath, string expectedHash) {
            if (string.IsNullOrWhiteSpace(expectedHash)) {
                Logger.Debug("VerifyFileSHA256: 预期哈希值为空，跳过验证");
                return true;
            }

            try {
                var actualHash = ComputeFileSHA256(filePath);
                var result = actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase);
                Logger.Debug($"VerifyFileSHA256: 实际={actualHash}, 预期={expectedHash.ToLowerInvariant()}, 结果={result}");
                return result;
            } catch (Exception ex) {
                Logger.Error($"VerifyFileSHA256 失败: {ex.Message}", ex);
                return false;
            }
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

        /// <summary>
        /// 等待进程退出（异步）
        /// </summary>
        private static async Task<bool> WaitForExitAsync(Process process, int timeoutMs) {
            var tcs = new TaskCompletionSource<bool>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(true);

            if (process.HasExited) {
                return true;
            }

            var timeoutTask = Task.Delay(timeoutMs);
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

            return completedTask == tcs.Task && process.HasExited;
        }

        /// <summary>
        /// 停止指定路径的进程（先优雅关闭，超时后强制终止）
        /// </summary>
        /// <param name="fullpath">进程可执行文件的完整路径</param>
        /// <param name="gracefulTimeoutMs">优雅关闭的超时时间（毫秒），默认 5000ms</param>
        /// <returns>异常信息，成功返回 null</returns>
        public static Exception? StopProcessByPath(string fullpath, int gracefulTimeoutMs = 5000) {
            Logger.Debug($"StopProcessByPath fullpath={fullpath} timeout={gracefulTimeoutMs}ms");
            var fileName = Path.GetFileName(fullpath);
            var moduleName = Path.GetFileNameWithoutExtension(fileName);

            try {
                Process[] existing = Process.GetProcessesByName(moduleName);
                var processesToStop = existing
                    .Where(p => p.MainModule?.FileName == fullpath)
                    .ToList();

                if (processesToStop.Count == 0) {
                    Logger.Debug("StopProcessByPath: 没有找到匹配的进程");
                    return null;
                }

                Logger.Debug($"StopProcessByPath: 找到 {processesToStop.Count} 个进程需要关闭");

                // 第一步：尝试优雅关闭（发送 WM_CLOSE 消息）
                foreach (Process p in processesToStop) {
                    try {
                        Logger.Debug($"StopProcessByPath: 尝试优雅关闭进程 PID={p.Id} {p.ProcessName}");
                        if (p.CloseMainWindow()) {
                            Logger.Debug($"StopProcessByPath: 成功发送关闭消息到 PID={p.Id}");
                        } else {
                            Logger.Debug($"StopProcessByPath: 进程 PID={p.Id} 没有主窗口，无法优雅关闭");
                        }
                    } catch (Exception ex) {
                        Logger.Debug($"StopProcessByPath: 优雅关闭进程 PID={p.Id} 失败: {ex.Message}");
                    }
                }

                // 第二步：等待进程优雅退出
                var waitTask = Task.Run(async () => {
                    foreach (Process p in processesToStop) {
                        try {
                            // 刷新进程状态
                            p.Refresh();
                            if (!p.HasExited) {
                                var exited = await WaitForExitAsync(p, gracefulTimeoutMs);
                                if (exited) {
                                    Logger.Debug($"StopProcessByPath: 进程 PID={p.Id} 已优雅退出");
                                }
                            }
                        } catch (Exception ex) {
                            Logger.Debug($"StopProcessByPath: 等待进程 PID={p.Id} 退出时出错: {ex.Message}");
                        }
                    }
                });

                // 同步等待异步任务完成
                waitTask.Wait(gracefulTimeoutMs + 1000);

                // 第三步：强制终止仍然运行的进程
                foreach (Process p in processesToStop) {
                    try {
                        p.Refresh();
                        if (!p.HasExited) {
                            Logger.Debug($"StopProcessByPath: 进程 PID={p.Id} 未响应，强制终止");
                            p.Kill();
                            p.WaitForExit(1000);
                            Logger.Debug($"StopProcessByPath: 进程 PID={p.Id} 已强制终止");
                        }
                    } catch (Exception ex) {
                        Logger.Debug($"StopProcessByPath: 强制终止进程 PID={p.Id} 失败: {ex.Message}");
                    }
                }

                // 第四步：清理资源
                foreach (Process p in processesToStop) {
                    try {
                        p.Dispose();
                    } catch { }
                }

                Logger.Debug("StopProcessByPath: 所有进程已关闭");
                return null;
            } catch (Exception ex) {
                Logger.Debug($"StopProcessByPath error={ex}");
                return ex;
            }
        }
    }
}