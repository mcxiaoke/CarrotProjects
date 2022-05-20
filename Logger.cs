using System;
using System.Text;

namespace GenshinNotifier {

    public static class Logger {
#if DEBUG
        public static ILogger Default = new ConsoleLogger();
#else
        public static ILogger Default = new DummyLogger();
#endif

        public static void Verbose(string m) => Default.Log(LoggerLevel.Verbose, m);
        public static void Debug(string m) => Default.Log(LoggerLevel.Debug, m);
        public static void Info(string m) => Default.Log(LoggerLevel.Info, m);
        public static void Warning(string m) => Default.Log(LoggerLevel.Warning, m);
        public static void Error(string m) => Default.Log(LoggerLevel.Error, m);

        public static void Fatal(string m) => Default.Log(LoggerLevel.Fatal, m);

        public static void Error(string m, Exception e) {
            var sb = new StringBuilder(m);
            sb.AppendLine(e.Message ?? "");
            Default.Log(LoggerLevel.Error, sb.ToString());
        }
    }
}
