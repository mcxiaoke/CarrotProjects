using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace Carrot.Common {

    public interface ILogger {

        void Log(LogEventLevel lv, string message);

        void Error(string message, Exception error);
    }

    internal abstract class BaseLogger : ILogger {

        public virtual void Log(LogEventLevel lv, string message) {
        }

        public virtual void Error(string message, Exception error) {
        }
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
            //Debug.WriteLine(message);
            if (message != null) {
                _logger.Write(lv, message);
            }
        }

        public override void Error(string? message, Exception error) {
            //Trace.WriteLine(message + " " + error.Message);
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

    public static class Logger {
#if DEBUG
        private static readonly ILogger Default = new DebugLogger();
#else
        private static readonly ILogger Default = new ReleaseLogger();
#endif

        public static void Verbose(string? m) => Default.Log(LogEventLevel.Verbose, m ?? string.Empty);

        public static void Debug(string? m) => Default.Log(LogEventLevel.Debug, m ?? string.Empty);

        public static void Info(string? m) => Default.Log(LogEventLevel.Information, m ?? string.Empty);

        public static void Warning(string? m) => Default.Log(LogEventLevel.Warning, m ?? string.Empty);

        public static void Error(string? m, Exception e) => Default.Error(m ?? string.Empty, e);

        public static void Close() => Log.CloseAndFlush();
    }
}