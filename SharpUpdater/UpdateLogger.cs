using System;
using System.IO;
using System.Text;
using Carrot.Common;

namespace SharpUpdater {

    /// <summary>
    /// 更新日志记录器 - 提供结构化日志记录
    /// </summary>
    public static class UpdateLogger {

        private static readonly object _lock = new object();
        private static string? _logFilePath;
        private static bool _enableFileLog = true;

        /// <summary>
        /// 日志文件路径
        /// </summary>
        public static string LogFilePath {
            get {
                if (_logFilePath == null) {
                    var logDir = Path.Combine(SharpConfig.AppBase, "logs");
                    Directory.CreateDirectory(logDir);
                    _logFilePath = Path.Combine(logDir, $"update_{DateTime.Now:yyyyMMdd}.log");
                }
                return _logFilePath;
            }
        }

        /// <summary>
        /// 是否启用文件日志
        /// </summary>
        public static bool EnableFileLog {
            get => _enableFileLog;
            set => _enableFileLog = value;
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        public static void LogInfo(string message, params object[] args) {
            Log("INFO", message, args);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        public static void LogWarning(string message, params object[] args) {
            Log("WARN", message, args);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        public static void LogError(string message, Exception? exception = null) {
            var fullMessage = exception != null ? $"{message}\nException: {exception}" : message;
            Log("ERROR", fullMessage);
        }

        /// <summary>
        /// 记录错误日志（格式化）
        /// </summary>
        public static void LogError(string message, params object[] args) {
            var formattedMessage = string.Format(message, args);
            Log("ERROR", formattedMessage);
        }

        /// <summary>
        /// 记录调试日志
        /// </summary>
        public static void LogDebug(string message, params object[] args) {
#if DEBUG
            Log("DEBUG", message, args);
#endif
        }

        /// <summary>
        /// 记录更新事件
        /// </summary>
        public static void LogUpdateEvent(string eventName, string? details = null) {
            var message = $"[UpdateEvent:{eventName}]";
            if (!string.IsNullOrEmpty(details)) {
                message += $" {details}";
            }
            LogInfo(message);
        }

        /// <summary>
        /// 记录更新成功
        /// </summary>
        public static void LogUpdateSuccess(string appName, string fromVersion, string toVersion) {
            LogUpdateEvent("UpdateSuccess", $"App={appName}, From={fromVersion}, To={toVersion}");
        }

        /// <summary>
        /// 记录更新失败
        /// </summary>
        public static void LogUpdateFailed(string appName, string reason, Exception? exception = null) {
            var message = $"App={appName}, Reason={reason}";
            LogUpdateEvent("UpdateFailed", message);
            if (exception != null) {
                LogError($"Update failed: {message}", exception);
            }
        }

        /// <summary>
        /// 记录下载事件
        /// </summary>
        public static void LogDownloadEvent(string url, long bytesDownloaded, long? totalBytes) {
            var message = $"URL={url}, Downloaded={bytesDownloaded}";
            if (totalBytes.HasValue) {
                var percent = (int)((double)bytesDownloaded / totalBytes.Value * 100);
                message += $", Total={totalBytes}, Progress={percent}%";
            }
            LogDebug(message);
        }

        /// <summary>
        /// 清理旧日志文件（保留最近N天）
        /// </summary>
        public static void CleanupOldLogs(int keepDays = 7) {
            try {
                var logDir = Path.GetDirectoryName(LogFilePath);
                if (logDir == null || !Directory.Exists(logDir)) return;

                var cutoffDate = DateTime.Now.AddDays(-keepDays);
                var logFiles = Directory.GetFiles(logDir, "update_*.log");

                foreach (var file in logFiles) {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate) {
                        try {
                            File.Delete(file);
                            Logger.Debug($"Deleted old log file: {file}");
                        } catch (Exception ex) {
                            Logger.Debug($"Failed to delete log file: {ex.Message}");
                        }
                    }
                }
            } catch (Exception ex) {
                Logger.Debug($"CleanupOldLogs failed: {ex.Message}");
            }
        }

        private static void Log(string level, string message, params object[] args) {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var threadId = Environment.CurrentManagedThreadId;

            var logEntry = $"[{timestamp}] [{level}] [Thread:{threadId}] {formattedMessage}";

            // 输出到控制台
            Console.WriteLine(logEntry);

            // 输出到 Carrot.Common.Logger
            Logger.Debug(logEntry);

            // 输出到文件
            if (_enableFileLog) {
                WriteToFile(logEntry);
            }
        }

        private static void WriteToFile(string logEntry) {
            try {
                lock (_lock) {
                    File.AppendAllText(LogFilePath, logEntry + "\n", Encoding.UTF8);
                }
            } catch (Exception ex) {
                Logger.Debug($"Failed to write log to file: {ex.Message}");
            }
        }
    }
}
