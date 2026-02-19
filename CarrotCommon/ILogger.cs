using System;
using System.IO;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Events;

namespace Carrot.Common;

/// <summary>
/// Interface for logging operations.
/// 日志操作接口。
/// </summary>
public interface ILogger {
    /// <summary>
    /// Logs a message with the specified level.
    /// 记录具有指定级别的消息。
    /// </summary>
    void Log(LogEventLevel lv, string message);

    /// <summary>
    /// Logs an error message with an exception.
    /// 记录带有异常的错误消息。
    /// </summary>
    void Error(string message, Exception error);
}

internal abstract class BaseLogger : ILogger {
    public virtual void Log(LogEventLevel lv, string message) { }
    public virtual void Error(string message, Exception error) { }
}

internal sealed class DebugLogger : BaseLogger {
    private readonly Serilog.ILogger _logger;

    public DebugLogger() {
        const string name = "debug.txt";
        var logOutput = Path.Combine(AppInfo.LocalAppDataPath, "logs");
        Storage.CheckOrCreateDir(logOutput);
        var logFile = Path.Combine(logOutput, name);
        
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.Async(a => a.File(logFile))
            .CreateLogger();
        
        Serilog.Log.Logger = _logger;
    }

    public override void Log(LogEventLevel lv, string? message) {
        if (message != null) {
            _logger.Write(lv, message);
        }
    }

    public override void Error(string? message, Exception error) {
        _logger.Error(error, message ?? "Error");
    }
}

internal sealed class ReleaseLogger : BaseLogger {
    private readonly Serilog.ILogger _logger;

    public ReleaseLogger() {
        const string name = "log-.txt";
        var logOutput = Path.Combine(AppInfo.LocalAppDataPath, "logs");
        Storage.CheckOrCreateDir(logOutput);
        var logFile = Path.Combine(logOutput, name);
        
        _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Async(a => a.File(logFile, rollingInterval: RollingInterval.Month))
            .CreateLogger();
        
        Serilog.Log.Logger = _logger;
    }

    public override void Log(LogEventLevel lv, string message) {
        _logger.Write(lv, message);
    }

    public override void Error(string message, Exception error) {
        _logger.Error(error, message);
    }
}

/// <summary>
/// Static logger helper.
/// 静态日志助手。
/// </summary>
public static class Logger {
#if DEBUG
    private static readonly ILogger Default = new DebugLogger();
#else
    private static readonly ILogger Default = new ReleaseLogger();
#endif

    private static string F(string? m, string member, string file, int line) {
        return $"[{Path.GetFileNameWithoutExtension(file)}.{member}:{line}] {m ?? string.Empty}";
    }

    /// <summary>
    /// Logs a verbose message.
    /// 记录详细消息。
    /// </summary>
    public static void Verbose(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Default.Log(LogEventLevel.Verbose, F(m, member, file, line));

    /// <summary>
    /// Logs a debug message.
    /// 记录调试消息。
    /// </summary>
    public static void Debug(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Default.Log(LogEventLevel.Debug, F(m, member, file, line));

    /// <summary>
    /// Logs an information message.
    /// 记录信息消息。
    /// </summary>
    public static void Info(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Default.Log(LogEventLevel.Information, F(m, member, file, line));

    /// <summary>
    /// Logs a warning message.
    /// 记录警告消息。
    /// </summary>
    public static void Warning(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Default.Log(LogEventLevel.Warning, F(m, member, file, line));

    /// <summary>
    /// Logs an error message with an exception.
    /// 记录带有异常的错误消息。
    /// </summary>
    public static void Error(string? m, Exception e, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Default.Error(F(m, member, file, line), e);

    /// <summary>
    /// Closes and flushes the logger.
    /// 关闭并刷新记录器。
    /// </summary>
    public static void Close() => Serilog.Log.CloseAndFlush();
}