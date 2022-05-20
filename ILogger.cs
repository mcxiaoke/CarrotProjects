using System;

namespace GenshinNotifier {
    public enum LoggerLevel {
        Verbose,
        Debug,
        Info,
        Warning,
        Error,
        Fatal,
    }

    public interface ILogger {
        int Level { get; set; }
        void Log(LoggerLevel lv, string message);

    }

    public abstract class BaseLogger : ILogger {
        protected int _level = (int)LoggerLevel.Info;
        public int Level {
            get => _level;
            set => _level = value;
        }

        public virtual void Log(LoggerLevel lv, string message) {
        }
    }

    public sealed class DummyLogger : BaseLogger {
    }

    public sealed class ConsoleLogger : BaseLogger {

        public override void Log(LoggerLevel lv, string message) {
            if ((int)lv >= (int)Level) {
                Console.WriteLine($"[{lv}] {message}");
            }
        }
    }
}
