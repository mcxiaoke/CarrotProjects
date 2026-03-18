using Carrot.Common;
using Newtonsoft.Json;
using Semver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SharpUpdater {

    /// <summary>
    /// 更新服务 - 处理所有更新相关的业务逻辑
    /// </summary>
    public class UpdateService {
        private readonly HttpClient _httpClient;
        private readonly int _downloadTimeout;

        public UpdateService(HttpClient httpClient, int downloadTimeoutSeconds = 30) {
            _httpClient = httpClient;
            _downloadTimeout = downloadTimeoutSeconds;
        }

        /// <summary>
        /// 清理旧的下载包文件
        /// </summary>
        /// <param name="keepDays">保留天数，默认7天</param>
        public static void CleanupOldPackages(int keepDays = 7) {
            try {
                var appBase = SharpConfig.AppBase;
                var cutoffDate = DateTime.Now.AddDays(-keepDays);
                var pattern = "UpdatePackage_*.zip";

                var files = Directory.GetFiles(appBase, pattern)
                    .Select(f => new FileInfo(f))
                    .Where(f => f.CreationTime < cutoffDate)
                    .ToList();

                foreach (var file in files) {
                    try {
                        file.Delete();
                        UpdateLogger.LogInfo("清理旧下载包: {0}", file.Name);
                    } catch (Exception ex) {
                        UpdateLogger.LogWarning("清理失败 {0}: {1}", file.Name, ex.Message);
                    }
                }

                if (files.Count > 0) {
                    UpdateLogger.LogInfo("已清理 {0} 个旧下载包", files.Count);
                }
            } catch (Exception ex) {
                UpdateLogger.LogError("清理旧下载包失败", ex);
            }
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <param name="versionUrl">版本信息 URL</param>
        /// <returns>更新检查结果</returns>
        public async Task<CheckUpdateResult> CheckUpdateAsync(string versionUrl) {
            try {
                UpdateLogger.LogInfo("开始检查更新: {0}", versionUrl);

                // 清理旧的下载包
                CleanupOldPackages(7);

                var text = await _httpClient.GetStringAsync(new Uri(versionUrl));
                var info = JsonConvert.DeserializeObject<VersionInfo>(text);

                if (info == null) {
                    UpdateLogger.LogError("无法解析版本信息");
                    return CheckUpdateResult.Failed(Strings.ErrorVersionInfoParseFailed);
                }

                if (VersionInfo.DataInValid(info)) {
                    UpdateLogger.LogError("版本信息无效: {0}", text);
                    return CheckUpdateResult.Failed(Strings.ErrorConfigInvalid(text));
                }

                var exePath = Path.Combine(SharpConfig.AppBase, info.Program);
                if (!File.Exists(exePath)) {
                    UpdateLogger.LogError("可执行文件不存在: {0}", exePath);
                    return CheckUpdateResult.Failed(Strings.ErrorExeNotFound(info.Program, SharpConfig.AppBase, info.Program));
                }

                var localFile = ReadFileVersion(exePath);
                if (localFile == null) {
                    UpdateLogger.LogError("无法读取本地版本信息");
                    return CheckUpdateResult.Failed(Strings.ErrorReadLocalVersionFailed);
                }

                var localVer = SemVersion.Parse(localFile.ProductVersion ?? "0.0.0", SemVersionStyles.Any);
                var remoteVer = SemVersion.Parse(info.Version, SemVersionStyles.Any);

                info.LocalName = localFile.ProductName ?? info.Name;
                info.LocalVersion = localFile.ProductVersion ?? "未知版本";

                bool hasNew = info.HasUpdate && localVer.ComparePrecedenceTo(remoteVer) < 0;

                UpdateLogger.LogInfo("检查更新完成: 本地版本={0}, 远程版本={1}, 有更新={2}",
                    localVer.ToString(), remoteVer.ToString(), hasNew);

                return CheckUpdateResult.Success(info, hasNew);
            } catch (Exception ex) {
                UpdateLogger.LogError("检查更新失败", ex);
                return CheckUpdateResult.Failed(Strings.ErrorCheckUpdateFailed(ex.Message, versionUrl, ex.ToString()));
            }
        }

        /// <summary>
        /// 下载更新包（支持断点续传）
        /// </summary>
        /// <param name="info">版本信息</param>
        /// <param name="progress">下载进度</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>下载结果</returns>
        public async Task<DownloadResult> DownloadPackageAsync(
            VersionInfo info,
            IProgress<float>? progress = null,
            CancellationToken cancellationToken = default) {
            string? filepath = null;
            try {
                var uri = new Uri(info.DownloadUrl);
                UpdateLogger.LogInfo("开始下载更新包: {0}", uri);

                filepath = Path.Combine(SharpConfig.AppBase, $"UpdatePackage_{info.Version}.zip");

                // 检查是否支持断点续传
                var supportsResume = await _httpClient.CheckResumeSupportAsync(uri);
                long downloadedBytes = 0;

                // 如果支持断点续传且文件已存在，获取已下载的字节数
                if (supportsResume && File.Exists(filepath)) {
                    var fileInfo = new FileInfo(filepath);
                    downloadedBytes = fileInfo.Length;
                    UpdateLogger.LogInfo("断点续传: 已下载 {0} 字节", downloadedBytes);
                } else if (File.Exists(filepath)) {
                    // 不支持断点续传，删除已存在的文件
                    File.Delete(filepath);
                    UpdateLogger.LogInfo("服务器不支持断点续传，重新下载");
                }

                // 下载文件（支持断点续传）
                using (var file = new FileStream(filepath, FileMode.Append, FileAccess.Write)) {
                    await _httpClient.DownloadWithResumeAsync(uri, file, downloadedBytes, progress, cancellationToken);
                }

                UpdateLogger.LogInfo("下载完成: {0}", filepath);
                return DownloadResult.Success(filepath);
            } catch (OperationCanceledException) {
                UpdateLogger.LogWarning("下载已取消");
                return DownloadResult.Failed(new OperationCanceledException("下载已取消"));
            } catch (Exception ex) {
                UpdateLogger.LogError("下载失败", ex);
                return DownloadResult.Failed(ex);
            }
        }

        /// <summary>
        /// 验证下载文件的 SHA256
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <param name="expectedHash">预期的哈希值</param>
        /// <returns>验证结果</returns>
        public static VerifyResult VerifyFileHash(string filepath, string expectedHash) {
            try {
                if (string.IsNullOrWhiteSpace(expectedHash)) {
                    UpdateLogger.LogInfo("SHA256 校验跳过: 未提供哈希值");
                    return VerifyResult.Skipped();
                }

                if (!File.Exists(filepath)) {
                    UpdateLogger.LogError("文件不存在: {0}", filepath);
                    return VerifyResult.Failed($"文件不存在: {filepath}");
                }

                var isValid = SharpUtils.VerifyFileSHA256(filepath, expectedHash);

                if (isValid) {
                    UpdateLogger.LogInfo("SHA256 校验成功");
                } else {
                    UpdateLogger.LogError("SHA256 校验失败: 文件可能已损坏");
                }

                return isValid ? VerifyResult.Success() : VerifyResult.Failed(Strings.ErrorFileCorrupted(filepath));
            } catch (Exception ex) {
                UpdateLogger.LogError("SHA256 校验异常", ex);
                return VerifyResult.Failed(ex.Message);
            }
        }

        /// <summary>
        /// 停止正在运行的程序
        /// </summary>
        /// <param name="info">版本信息</param>
        /// <returns>停止结果</returns>
        public static StopProcessResult StopRunningProcess(VersionInfo info) {
            try {
                var fullpath = Path.Combine(SharpConfig.AppBase, info.Program);
                UpdateLogger.LogInfo("停止进程: {0}", fullpath);

                var error = SharpUtils.StopProcessByPath(fullpath);

                if (error == null) {
                    UpdateLogger.LogInfo("进程已成功停止");
                } else {
                    UpdateLogger.LogError("停止进程失败", error);
                }

                return error == null ? StopProcessResult.Success() : StopProcessResult.Failed(error);
            } catch (Exception ex) {
                UpdateLogger.LogError("停止进程异常", ex);
                return StopProcessResult.Failed(ex);
            }
        }

        /// <summary>
        /// 安装更新
        /// </summary>
        /// <param name="info">版本信息</param>
        /// <param name="packagePath">安装包路径</param>
        /// <returns>安装结果</returns>
        public static async Task<InstallResult> InstallUpdateAsync(VersionInfo info, string packagePath) {
            var program = info.Program;
            var zipPath = Path.GetFullPath(packagePath);
            var destPath = Path.GetFullPath(SharpConfig.AppBase);

            UpdateLogger.LogInfo("开始安装更新: {0}", packagePath);

            // 创建备份管理器
            var backupManager = new UpdateBackupManager(destPath);

            return await Task.Run(() => {
                try {
                    var found = SharpUtils.ZipFileFind(zipPath, program);
                    if (found == null) {
                        UpdateLogger.LogError("安装包中未找到可执行文件: {0}", program);
                        return InstallResult.Failed(Strings.ErrorPackageExeNotFound(program));
                    }

                    bool strip = found.Contains('/') && found.Contains(program);
                    string stripPrefix = found.Replace(program, "");

                    // 获取需要备份的文件列表
                    var filesToBackup = GetFilesToBackup(zipPath, destPath, strip, stripPrefix);

                    // 创建备份（在解压前）
                    if (filesToBackup.Count > 0) {
                        try {
                            var backupId = backupManager.CreateBackup(filesToBackup);
                            UpdateLogger.LogInfo("已创建备份: {0}", backupId);
                        } catch (Exception ex) {
                            UpdateLogger.LogWarning("创建备份失败，继续更新: {0}", ex.Message);
                        }
                    }

                    // 解压文件
                    UpdateLogger.LogInfo("开始解压文件...");
                    SharpUtils.UnzipFile(zipPath, destPath, false, strip, stripPrefix);

                    // 清理旧备份
                    backupManager.CleanupOldBackups(3);

                    UpdateLogger.LogUpdateSuccess(info.Name, info.LocalVersion, info.Version);
                    return InstallResult.Success();
                } catch (Exception ex) {
                    UpdateLogger.LogError("安装失败", ex);

                    // 尝试回滚
                    try {
                        UpdateLogger.LogWarning("尝试回滚到之前的版本...");
                        if (backupManager.RestoreBackup()) {
                            UpdateLogger.LogInfo("回滚成功");
                        } else {
                            UpdateLogger.LogWarning("回滚失败或没有可用备份");
                        }
                    } catch (Exception rollbackEx) {
                        UpdateLogger.LogError("回滚过程中出错", rollbackEx);
                    }

                    return InstallResult.Failed(ex);
                }
            });
        }

        /// <summary>
        /// 获取需要备份的文件列表
        /// </summary>
        private static List<string> GetFilesToBackup(string zipPath, string destPath, bool strip, string stripPrefix) {
            var files = new List<string>();
            try {
                using var archive = System.IO.Compression.ZipFile.OpenRead(zipPath);
                foreach (var entry in archive.Entries) {
                    if (entry.Length == 0) continue;

                    var fullName = entry.FullName;
                    if (!string.IsNullOrWhiteSpace(stripPrefix) && fullName.StartsWith(stripPrefix)) {
                        fullName = fullName.Remove(0, stripPrefix.Length);
                    }

                    if (string.IsNullOrWhiteSpace(fullName)) continue;

                    var destFile = Path.Combine(destPath, fullName);
                    if (File.Exists(destFile)) {
                        files.Add(fullName);
                    }
                }
            } catch (Exception ex) {
                UpdateLogger.LogWarning("获取备份文件列表失败: {0}", ex.Message);
            }
            return files;
        }

        private static FileVersionInfo? ReadFileVersion(string path) {
            try {
                return FileVersionInfo.GetVersionInfo(path);
            } catch (Exception ex) {
                Logger.Error("ReadFileVersion", ex);
                return null;
            }
        }
    }

    // ========== 结果类型 ==========

    public class CheckUpdateResult {
        public bool IsSuccess { get; }
        public bool HasUpdate { get; }
        public VersionInfo? VersionInfo { get; }
        public string? ErrorMessage { get; }

        private CheckUpdateResult(bool isSuccess, bool hasUpdate, VersionInfo? info, string? error) {
            IsSuccess = isSuccess;
            HasUpdate = hasUpdate;
            VersionInfo = info;
            ErrorMessage = error;
        }

        public static CheckUpdateResult Success(VersionInfo info, bool hasUpdate) =>
            new CheckUpdateResult(true, hasUpdate, info, null);

        public static CheckUpdateResult Failed(string error) =>
            new CheckUpdateResult(false, false, null, error);
    }

    public class DownloadResult {
        public bool IsSuccess { get; }
        public string? FilePath { get; }
        public Exception? Error { get; }

        private DownloadResult(bool isSuccess, string? path, Exception? error) {
            IsSuccess = isSuccess;
            FilePath = path;
            Error = error;
        }

        public static DownloadResult Success(string path) =>
            new DownloadResult(true, path, null);

        public static DownloadResult Failed(Exception error) =>
            new DownloadResult(false, null, error);
    }

    public class VerifyResult {
        public bool IsSuccess { get; }
        public bool IsSkipped { get; }
        public string? ErrorMessage { get; }

        private VerifyResult(bool isSuccess, bool isSkipped, string? error) {
            IsSuccess = isSuccess;
            IsSkipped = isSkipped;
            ErrorMessage = error;
        }

        public static VerifyResult Success() => new VerifyResult(true, false, null);
        public static VerifyResult Skipped() => new VerifyResult(true, true, null);
        public static VerifyResult Failed(string error) => new VerifyResult(false, false, error);
    }

    public class StopProcessResult {
        public bool IsSuccess { get; }
        public Exception? Error { get; }

        private StopProcessResult(bool isSuccess, Exception? error) {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static StopProcessResult Success() => new StopProcessResult(true, null);
        public static StopProcessResult Failed(Exception error) => new StopProcessResult(false, error);
    }

    public class InstallResult {
        public bool IsSuccess { get; }
        public Exception? Error { get; }

        private InstallResult(bool isSuccess, Exception? error) {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static InstallResult Success() => new InstallResult(true, null);
        public static InstallResult Failed(Exception error) => new InstallResult(false, error);
        public static InstallResult Failed(string errorMessage) => new InstallResult(false, new Exception(errorMessage));
    }
}
