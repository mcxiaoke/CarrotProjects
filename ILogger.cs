using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace GenshinNotifier {
    public enum LoggerEvent {
        Verbose,
        Debug,
        Info,
        Warning,
        Error,
    }

    public interface ILogger {
        void Log(LoggerEvent loggerEvent, string message);
        
    }

    public sealed class DummyLogger : ILogger {
        public void Log(LoggerEvent loggerEvent, string message) { }
    }

    public sealed class ConsoleLogger : ILogger {

        public void Log(LoggerEvent loggerEvent, string message) {
            Console.WriteLine($"[{loggerEvent}] {message}");
        }
    }
}
