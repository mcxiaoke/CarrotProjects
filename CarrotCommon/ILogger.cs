using System;
using System.IO;
using System.Diagnostics;
using Serilog;
using Serilog.Events;

namespace CarrotCommon {

    public interface ILogger {
        void Log(LogEventLevel lv, string message);
        void Error(string message, Exception error);
    }

    abstract class BaseLogger : ILogger {

        public virtual void Log(LogEventLevel lv, string message) {
        }
        public virtual void Error(string message, Exception error) { }
    }

    sealed class DebugLogger : BaseLogger {
        private readonly Serilog.ILogger _logger;

        public DebugLogger() {
            var name = "debug.txt";
            var logOutput = Path.Combine(Storage.UserDataFolder, "logs");
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

        public override void Log(LogEventLevel lv, string message) {
            //Debug.WriteLine(message);
            _logger.Write(lv, message);
        }
        public override void Error(string message, Exception error) {
            //Trace.WriteLine(message + " " + error.Message);
            _logger.Error(error, message);
        }
    }

    sealed class ReleaseLogger : BaseLogger {
        private readonly Serilog.ILogger _logger;

        public ReleaseLogger() {
            var name = "log-.txt";
            var logOutput = Path.Combine(Storage.UserDataFolder, "logs");
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
        static readonly ILogger Default = new DebugLogger();
#else
        static readonly ILogger Default = new ReleaseLogger();
#endif

        public static void Verbose(string m) => Default.Log(LogEventLevel.Verbose, m);
        public static void Debug(string m) => Default.Log(LogEventLevel.Debug, m);
        public static void Info(string m) => Default.Log(LogEventLevel.Information, m);
        public static void Warning(string m) => Default.Log(LogEventLevel.Warning, m);
        public static void Error(string m, Exception e) => Default.Error(m, e);
        public static void Close() => Log.CloseAndFlush();
    }
}
