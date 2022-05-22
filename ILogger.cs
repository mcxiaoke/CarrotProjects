using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace GenshinNotifier {

    public interface ILogger {
        void Log(LogEventLevel lv, string message);
        void Error(string message, Exception error);
    }

    abstract class BaseLogger : ILogger {

        public virtual void Log(LogEventLevel lv, string message) {
        }
        public virtual void Error(string message, Exception error) { }
    }

    sealed class DummyLogger : BaseLogger {
    }

    sealed class SeriLogger : BaseLogger {
        private readonly Serilog.ILogger _logger;

        public SeriLogger() {
            var logOutput = Path.Combine(Storage.UserDataFolder, "logs");
            Storage.CheckOrCreateDir(logOutput);
            var logFile = Path.Combine(logOutput, "log-.txt");
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Async(a => a.File(logFile, restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Day))
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
//#if DEBUG
        static readonly ILogger Default = new SeriLogger();
//#else
//         static ILogger Default = new DummyLogger();
//#endif

        public static void Verbose(string m) => Default.Log(LogEventLevel.Verbose, m);
        public static void Debug(string m) => Default.Log(LogEventLevel.Debug, m);
        public static void Info(string m) => Default.Log(LogEventLevel.Information, m);
        public static void Warning(string m) => Default.Log(LogEventLevel.Warning, m);
        public static void Error(string m, Exception e) => Default.Error(m, e);
        public static void Close() => Serilog.Log.CloseAndFlush();
    }
}
