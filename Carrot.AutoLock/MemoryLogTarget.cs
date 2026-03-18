using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Carrot.Common;
using Serilog.Core;
using Serilog.Events;

namespace Carrot.AutoLock;

/// <summary>
/// 内存日志目标，用于在内存中存储日志记录
/// Memory log target for storing log records in memory
/// </summary>
public class MemoryLogTarget : ILogSink {
    private readonly ConcurrentQueue<LogEntry> _logQueue;
    private readonly int _maxLines;
    private readonly object _lock = new();
    private bool _disposed;

    /// <summary>
    /// 日志更新事件
/// Log updated event
    /// </summary>
    public event EventHandler<LogUpdatedEventArgs>? LogUpdated;

    /// <summary>
    /// 获取当前日志行数
    /// Get current log line count
    /// </summary>
    public int LineCount => _logQueue.Count;

    /// <summary>
    /// 获取 Serilog ILogEventSink 实现
    /// Gets the Serilog ILogEventSink implementation
    /// </summary>
    public ILogEventSink Sink => _sink;

    private readonly MemoryLogSink _sink;

    /// <summary>
    /// 初始化内存日志目标
    /// Initialize memory log target
    /// </summary>
    /// <param name="maxLines">最大日志行数，超过则删除最早的日志 / Max log lines, removes oldest when exceeded</param>
    public MemoryLogTarget(int maxLines = 10000) {
        _logQueue = new ConcurrentQueue<LogEntry>();
        _maxLines = maxLines;
        _sink = new MemoryLogSink(this);
    }

    internal void EmitInternal(LogEvent logEvent) {
        if (_disposed) return;

        try {
            // 格式化日志消息
            var timestamp = logEvent.Timestamp.ToString("HH:mm:ss.fff");
            // 简化日志级别显示
            var level = logEvent.Level switch {
                LogEventLevel.Verbose => "VRB",
                LogEventLevel.Debug => "DBG",
                LogEventLevel.Information => "INF",
                LogEventLevel.Warning => "WRN",
                LogEventLevel.Error => "ERR",
                LogEventLevel.Fatal => "FTL",
                _ => logEvent.Level.ToString().ToUpper()[..3]
            };
            var message = logEvent.RenderMessage();

            var entry = new LogEntry(timestamp, level, message);

            // 如果有异常，添加异常信息到消息
            if (logEvent.Exception != null) {
                entry = new LogEntry(timestamp, level, $"{message}\n{logEvent.Exception}");
            }

            // 添加到队列
            _logQueue.Enqueue(entry);

            // 检查是否超过最大行数
            while (_logQueue.Count > _maxLines) {
                _logQueue.TryDequeue(out _);
            }

            // 触发日志更新事件
            LogUpdated?.Invoke(this, new LogUpdatedEventArgs(entry.ToString()));
        } catch (Exception) {
            // 防止日志记录本身导致崩溃
        }
    }

    /// <summary>
    /// 获取所有日志条目
    /// Get all log entries
    /// </summary>
    public IReadOnlyList<LogEntry> GetAllLogs() {
        return _logQueue.ToArray();
    }

    /// <summary>
    /// 获取最近的日志条目
    /// Get recent log entries
    /// </summary>
    /// <param name="lines">行数 / Number of lines</param>
    public IReadOnlyList<LogEntry> GetLogLines(int lines = 1000) {
        var allLogs = _logQueue.ToArray();
        var startIndex = Math.Max(0, allLogs.Length - lines);
        var result = new LogEntry[allLogs.Length - startIndex];
        Array.Copy(allLogs, startIndex, result, 0, result.Length);
        return result;
    }

    /// <summary>
    /// 清空所有日志
    /// Clear all logs
    /// </summary>
    public void Clear() {
        while (_logQueue.TryDequeue(out _)) { }
        Common.Logger.Info("Memory log cleared by user");
    }

    public void Dispose() {
        if (_disposed) return;
        _disposed = true;
        Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Internal sink wrapper for Serilog integration
    /// </summary>
    private sealed class MemoryLogSink : ILogEventSink {
        private readonly MemoryLogTarget _target;

        public MemoryLogSink(MemoryLogTarget target) {
            _target = target;
        }

        public void Emit(LogEvent logEvent) {
            _target.EmitInternal(logEvent);
        }
    }
}

/// <summary>
/// 日志条目
/// Log entry
/// </summary>
public readonly struct LogEntry {
    public string Timestamp { get; }
    public string Level { get; }
    public string Message { get; }

    public LogEntry(string timestamp, string level, string message) {
        Timestamp = timestamp;
        Level = level;
        Message = message;
    }

    public override string ToString() => $"[{Timestamp}] [{Level}] {Message}";
}

/// <summary>
/// 日志更新事件参数
/// Log updated event arguments
/// </summary>
public class LogUpdatedEventArgs : EventArgs {
    public string NewLogLine { get; }

    public LogUpdatedEventArgs(string newLogLine) {
        NewLogLine = newLogLine;
    }
}
