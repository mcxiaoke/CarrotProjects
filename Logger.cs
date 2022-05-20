using System;
using System.Text;
using Serilog.Events;

namespace GenshinNotifier {

    public static class Logger {
#if DEBUG
        public static ILogger Default = new SeriLogger();
#else
        public static ILogger Default = new DummyLogger();
#endif

        public static void Verbose(string m) => Default.Log(LogEventLevel.Verbose, m);
        public static void Debug(string m) => Default.Log(LogEventLevel.Debug, m);
        public static void Info(string m) => Default.Log(LogEventLevel.Information, m);
        public static void Warning(string m) => Default.Log(LogEventLevel.Warning, m);
        public static void Error(string m, Exception e) => Default.Error(m, e);
    }
}
