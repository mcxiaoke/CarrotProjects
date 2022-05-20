using System;
using Serilog;
using Serilog.Events;

namespace GenshinNotifier {

    public interface ILogger {
        void Log(LogEventLevel lv, string message);
        void Error(string message, Exception error);

    }

    public abstract class BaseLogger : ILogger {

        public virtual void Log(LogEventLevel lv, string message) {
        }
        public virtual void Error(string message, Exception error) { }
    }

    public sealed class DummyLogger : BaseLogger {
    }

    public sealed class SeriLogger : BaseLogger {
        private readonly Serilog.ILogger _logger;

        public SeriLogger() {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
        }

        public override void Log(LogEventLevel lv, string message) {
            _logger.Write(lv, message);
        }

        public override void Error(string message, Exception error) {
            _logger.Error(error, message);
        }
    }
}
