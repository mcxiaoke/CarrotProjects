using Carrot.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Carrot.AutoLock;

/// <summary>
/// 进程检测器
/// 检测指定进程是否正在运行，支持检测管理员权限启动的进程
/// Process checker that detects if specified processes are running,
/// including processes running with administrator privileges
/// </summary>
public class ProcessChecker {

    /// <summary>
    /// 检查是否有任何豁免进程正在运行
    /// Check if any exempt process is currently running
    /// </summary>
    /// <param name="processNames">进程名称列表（自动处理 .exe 后缀）</param>
    /// <returns>如果有任何进程正在运行则返回 true</returns>
    public static bool IsAnyProcessRunning(IEnumerable<string> processNames) {
        if (processNames == null || !processNames.Any()) {
            return false;
        }

        // 自动 strip .exe 后缀
        var nameSet = new HashSet<string>(
            processNames.Select(NormalizeProcessName),
            StringComparer.OrdinalIgnoreCase
        );

        // 移除空字符串
        nameSet.Remove("");

        if (nameSet.Count == 0) {
            return false;
        }

        try {
            var allProcesses = Process.GetProcesses();

            foreach (var process in allProcesses) {
                try {
                    var processName = process.ProcessName;

                    if (nameSet.Contains(processName)) {
                        Logger.Debug($"Exempt process detected: {processName} (PID: {process.Id})");
                        return true;
                    }
                } catch (Win32Exception) {
                    // 访问某些系统进程或管理员进程时可能抛出异常
                    // 但 ProcessName 通常可以访问，这里忽略
                } catch (Exception ex) {
                    Logger.Debug($"Error checking process: {ex.Message}");
                } finally {
                    process.Dispose();
                }
            }
        } catch (Exception ex) {
            Logger.Error("Failed to enumerate processes", ex);
        }

        return false;
    }

    /// <summary>
    /// 标准化进程名称（移除 .exe 后缀）
    /// Normalize process name (remove .exe suffix)
    /// </summary>
    private static string NormalizeProcessName(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            return "";
        }
        var trimmed = name.Trim();
        if (trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            return trimmed[..^4];
        }
        return trimmed;
    }

    /// <summary>
    /// 获取所有正在运行的进程名称列表
    /// Get list of all running process names
    /// </summary>
    /// <returns>进程名称列表</returns>
    public static List<string> GetRunningProcessNames() {
        var names = new List<string>();

        try {
            var processes = Process.GetProcesses();
            foreach (var process in processes) {
                try {
                    names.Add(process.ProcessName);
                } catch {
                    // 忽略无法访问的进程
                } finally {
                    process.Dispose();
                }
            }
        } catch (Exception ex) {
            Logger.Error("Failed to get running processes", ex);
        }

        return names.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToList();
    }

    /// <summary>
    /// 检查指定进程是否正在运行
    /// Check if a specific process is running
    /// </summary>
    /// <param name="processName">进程名称（不含 .exe 后缀）</param>
    /// <returns>如果进程正在运行则返回 true</returns>
    public static bool IsProcessRunning(string processName) {
        if (string.IsNullOrWhiteSpace(processName)) {
            return false;
        }

        try {
            var processes = Process.GetProcessesByName(processName);
            var isRunning = processes.Length > 0;

            foreach (var p in processes) {
                p.Dispose();
            }

            return isRunning;
        } catch (Exception ex) {
            Logger.Debug($"Error checking process {processName}: {ex.Message}");
            return false;
        }
    }

    #region Native Methods for elevated process detection

    // 以下 API 用于更精确地检测进程是否以管理员权限运行
    // 但对于简单的"进程是否运行"检测，上面的方法已经足够

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr processHandle, int desiredAccess, out IntPtr tokenHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetTokenInformation(IntPtr tokenHandle, int tokenInformationClass,
        out int tokenInformation, int tokenInformationLength, out int returnLength);

    private const int TOKEN_QUERY = 0x0008;
    private const int TokenElevation = 20;

    /// <summary>
    /// 检查当前进程是否以管理员权限运行
    /// Check if the current process is running with administrator privileges
    /// </summary>
    public static bool IsCurrentProcessElevated() {
        try {
            using var process = Process.GetCurrentProcess();
            if (OpenProcessToken(process.Handle, TOKEN_QUERY, out var tokenHandle)) {
                try {
                    if (GetTokenInformation(tokenHandle, TokenElevation, out var elevation, sizeof(int), out _)) {
                        return elevation != 0;
                    }
                } finally {
                    CloseHandle(tokenHandle);
                }
            }
        } catch (Exception ex) {
            Logger.Debug($"Failed to check elevation status: {ex.Message}");
        }
        return false;
    }

    #endregion
}
