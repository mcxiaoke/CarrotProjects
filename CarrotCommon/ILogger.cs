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

    static Logger() {
        InitializeLogger();
    }

    private static void InitializeLogger() {
        var logOutput = Path.Combine(AppInfo.LocalAppDataPath, "logs");
        Storage.CheckOrCreateDir(logOutput);

#if DEBUG
        // Debug 模式：日志文件名为 debug-YYYYMMDD.txt，每天一个文件
        var logFile = Path.Combine(logOutput, "debug-log-.txt");

        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.Async(a => a.File(logFile, rollingInterval: RollingInterval.Day))
            .CreateLogger();
#else
        // Release 模式：日志文件名为 release-YYYYMMDD.txt，每天一个文件
        var logFile = Path.Combine(logOutput, "release-log-.txt");

        _logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Async(a => a.File(logFile, rollingInterval: RollingInterval.Day))
            .CreateLogger();
#endif
        Serilog.Log.Logger = _logger;
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
        var existingLogger = Serilog.Log.Logger;
        var config = new LoggerConfiguration()
            .MinimumLevel.Is(existingLogger.IsEnabled(LogEventLevel.Debug)
                ? LogEventLevel.Debug
                : LogEventLevel.Information)
            .WriteTo.Logger(existingLogger);

        foreach (var sink in _registeredSinks) {
            config.WriteTo.Sink(sink.Sink);
        }

        Serilog.Log.Logger = config.CreateLogger();
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