using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Carrot.Common;

/// <summary>
/// Interface for custom log sinks that can be added to the logger.
/// 可添加到记录器的自定义日志接收器接口。
/// </summary>
public interface ILogSink : IDisposable {
    /// <summary>
    /// Gets the Serilog ILogEventSink implementation.
    /// 获取 Serilog ILogEventSink 实现。
    /// </summary>
    Serilog.Core.ILogEventSink Sink { get; }
}

/// <summary>
/// Static logger helper.
/// 静态日志助手。
/// </summary>
public static class Logger {
    private static readonly List<ILogSink> _registeredSinks = new();
    private static readonly object _lock = new();
    private static Serilog.ILogger _logger = null!;
    private static Serilog.ILogger _loggerNoFile = null!;
    private static LogEventLevel _minimumLevel;

    /// <summary>
    /// 获取当前最小日志级别
    /// Get current minimum log level
    /// </summary>
    public static LogEventLevel MinimumLevel => _minimumLevel;

    /// <summary>
    /// 是否启用详细日志（Debug 级别）
    /// Whether verbose logging (Debug level) is enabled
    /// </summary>
    public static bool IsVerboseEnabled => _minimumLevel <= LogEventLevel.Debug;

    static Logger() {
        InitializeLogger();
    }

    private static void InitializeLogger() {
        var logOutput = Path.Combine(AppInfo.LocalAppDataPath, "logs");
        Storage.CheckOrCreateDir(logOutput);

#if DEBUG
        _minimumLevel = LogEventLevel.Debug;
        // Debug 模式：日志文件名为 debug-YYYYMMDD.txt，每天一个文件
        var logFile = Path.Combine(logOutput, "debug-log-.txt");

        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.Async(a => a.File(logFile, rollingInterval: RollingInterval.Day))
            .CreateLogger();

        // 不写文件的 logger（用于频繁的状态日志）
        _loggerNoFile = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.Console()
            .CreateLogger();
#else
        _minimumLevel = LogEventLevel.Information;
        // Release 模式：日志文件名为 release-YYYYMMDD.txt，每天一个文件
        var logFile = Path.Combine(logOutput, "release-log-.txt");

        _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Async(a => a.File(logFile, rollingInterval: RollingInterval.Day))
            .CreateLogger();

        // 不写文件的 logger
        _loggerNoFile = new LoggerConfiguration()
            .MinimumLevel.Information()
            .CreateLogger();
#endif
        Serilog.Log.Logger = _logger;
    }

    /// <summary>
    /// 设置最小日志级别
    /// Set minimum log level
    /// </summary>
    /// <param name="level">日志级别 / Log level</param>
    public static void SetMinimumLevel(LogEventLevel level) {
        lock (_lock) {
            _minimumLevel = level;
            RebuildLogger();
        }
    }

    /// <summary>
    /// 启用或禁用详细日志（Debug 级别）
    /// Enable or disable verbose logging (Debug level)
    /// </summary>
    /// <param name="enabled">是否启用 / Whether to enable</param>
    public static void SetVerboseLogging(bool enabled) {
        SetMinimumLevel(enabled ? LogEventLevel.Debug : LogEventLevel.Information);
    }

    private static string F(string? m, string member, string file, int line) {
        return $"[{Path.GetFileNameWithoutExtension(file)}.{member}:{line}] {m ?? string.Empty}";
    }

    /// <summary>
    /// Logs a verbose message.
    /// 记录详细消息。
    /// </summary>
    public static void Verbose(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Serilog.Log.Logger.Write(LogEventLevel.Verbose, F(m, member, file, line));

    /// <summary>
    /// Logs a debug message.
    /// 记录调试消息。
    /// </summary>
    public static void Debug(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Serilog.Log.Logger.Write(LogEventLevel.Debug, F(m, member, file, line));

    /// <summary>
    /// Logs an information message.
    /// 记录信息消息。
    /// </summary>
    public static void Info(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Serilog.Log.Logger.Write(LogEventLevel.Information, F(m, member, file, line));

    /// <summary>
    /// Logs an information message with option to skip file logging.
    /// 记录信息消息，可选择跳过文件日志。
    /// </summary>
    /// <param name="m">Message to log / 要记录的消息</param>
    /// <param name="logToFile">Whether to log to file (default true) / 是否记录到文件（默认为 true）</param>
    /// <param name="member">Caller member name / 调用方成员名</param>
    /// <param name="file">Caller file path / 调用方文件路径</param>
    /// <param name="line">Caller line number / 调用方行号</param>
    public static void ConsoleInfo(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
        var formattedMessage = F(m, member, file, line);
        // 只输出到非文件 sink（console、debug 等）
        _loggerNoFile.Write(LogEventLevel.Information, formattedMessage);
    }

    /// <summary>
    /// Logs a warning message.
    /// 记录警告消息。
    /// </summary>
    public static void Warning(string? m, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Serilog.Log.Logger.Write(LogEventLevel.Warning, F(m, member, file, line));

    /// <summary>
    /// Logs an error message with an exception.
    /// 记录带有异常的错误消息。
    /// </summary>
    public static void Error(string? m, Exception e, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Serilog.Log.Logger.Error(e, F(m, member, file, line));

    /// <summary>
    /// Logs an error message with a short exception summary (no full stack trace).
    /// 记录带有异常摘要的错误消息（不含完整堆栈）。
    /// </summary>
    public static void ErrorShort(string? m, Exception e, [CallerMemberName] string member = "", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
        => Serilog.Log.Logger.Write(LogEventLevel.Error, F($"{m}: {GetExceptionSummary(e)}", member, file, line));

    /// <summary>
    /// Gets a short summary of an exception (type, message, inner exceptions).
    /// 获取异常的简短摘要（类型、消息、内部异常）。
    /// </summary>
    public static string GetExceptionSummary(Exception e) {
        var sb = new System.Text.StringBuilder();
        var current = e;
        var depth = 0;

        while (current != null && depth < 3) {
            if (depth > 0) sb.Append(" --> ");
            sb.Append($"{current.GetType().Name}: {current.Message}");
            current = current.InnerException;
            depth++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets key frames from exception stack trace (user code only).
    /// 从异常堆栈中提取关键帧（仅用户代码）。
    /// </summary>
    public static string GetExceptionKeyFrames(Exception e, int maxFrames = 3) {
        if (e.StackTrace == null) return string.Empty;

        var frames = e.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        var keyFrames = new List<string>();
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;

        foreach (var frame in frames) {
            if (keyFrames.Count >= maxFrames) break;
            var trimmed = frame.Trim();
            if (trimmed.Contains(currentDir) || trimmed.Contains("Carrot")) {
                var start = trimmed.IndexOf("at ") + 3;
                if (start > 2) {
                    var methodPart = trimmed.Substring(start);
                    var parenIdx = methodPart.IndexOf(')');
                    if (parenIdx > 0) {
                        keyFrames.Add(methodPart.Substring(0, parenIdx + 1));
                    } else {
                        keyFrames.Add(methodPart.Split(new[] { " in " }, StringSplitOptions.None)[0]);
                    }
                }
            }
        }

        return keyFrames.Count > 0 ? string.Join(" -> ", keyFrames) : string.Empty;
    }

    /// <summary>
    /// Adds a custom log sink to the logger.
    /// 向记录器添加自定义日志接收器。
    /// </summary>
    /// <param name="sink">The log sink to add. / 要添加的日志接收器</param>
    /// <returns>The added sink for chaining. / 添加的接收器，用于链式调用</returns>
    public static ILogSink AddSink(ILogSink sink) {
        lock (_lock) {
            _registeredSinks.Add(sink);
            RebuildLogger();
            return sink;
        }
    }

    /// <summary>
    /// Adds a Serilog ILogEventSink directly to the logger.
    /// 直接向记录器添加 Serilog ILogEventSink。
    /// </summary>
    /// <param name="sink">The Serilog sink to add. / 要添加的 Serilog 接收器</param>
    /// <returns>A disposable wrapper for the sink. / 接收器的可释放包装器</returns>
    public static IDisposable AddSink(Serilog.Core.ILogEventSink sink) {
        var wrapper = new SerilogSinkWrapper(sink);
        return AddSink(wrapper);
    }

    /// <summary>
    /// Removes a previously added log sink.
    /// 移除之前添加的日志接收器。
    /// </summary>
    /// <param name="sink">The sink to remove. / 要移除的接收器</param>
    public static void RemoveSink(ILogSink sink) {
        lock (_lock) {
            if (_registeredSinks.Remove(sink)) {
                sink.Dispose();
                RebuildLogger();
            }
        }
    }

    /// <summary>
    /// Gets all registered custom sinks.
    /// 获取所有已注册的自定义接收器。
    /// </summary>
    public static IReadOnlyList<ILogSink> GetSinks() => _registeredSinks.AsReadOnly();

    private static void RebuildLogger() {
        var config = new LoggerConfiguration()
            .MinimumLevel.Is(_minimumLevel)
            .WriteTo.Logger(_logger);

        foreach (var sink in _registeredSinks) {
            config.WriteTo.Sink(sink.Sink);
        }

        Serilog.Log.Logger = config.CreateLogger();

        // 同时更新 _loggerNoFile，添加所有自定义 sink 但不包含文件 sink
        var configNoFile = new LoggerConfiguration()
            .MinimumLevel.Is(_minimumLevel);

#if DEBUG
        configNoFile.WriteTo.Debug().WriteTo.Console();
#endif

        foreach (var sink in _registeredSinks) {
            configNoFile.WriteTo.Sink(sink.Sink);
        }

        _loggerNoFile = configNoFile.CreateLogger();
    }

    /// <summary>
    /// Closes and flushes the logger.
    /// 关闭并刷新记录器。
    /// </summary>
    public static void Close() {
        lock (_lock) {
            foreach (var sink in _registeredSinks) {
                try {
                    sink.Dispose();
                } catch { /* Ignore disposal errors */ }
            }
            _registeredSinks.Clear();
        }
        Serilog.Log.CloseAndFlush();
    }
}

/// <summary>
/// Wrapper for Serilog ILogEventSink to implement ILogSink.
/// Serilog ILogEventSink 的包装器，实现 ILogSink 接口。
/// </summary>
internal sealed class SerilogSinkWrapper : ILogSink {
    private readonly Serilog.Core.ILogEventSink _sink;
    private bool _disposed;

    public Serilog.Core.ILogEventSink Sink => _sink;

    public SerilogSinkWrapper(Serilog.Core.ILogEventSink sink) {
        _sink = sink;
    }

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;
        if (_sink is IDisposable disposable) {
            disposable.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}