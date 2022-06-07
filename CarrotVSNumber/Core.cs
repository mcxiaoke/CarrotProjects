using System;

namespace CarrotVSNumber {

    internal class Core {

        public static void Log(params string[] strings) {
            var message = string.Join(" ", strings);
            Console.WriteLine($"CarrotVSNumber: {message}");
        }

        public static void Error(params string[] strings) {
            var message = string.Join(" ", strings);
            Console.WriteLine($"CarrotVSNumber Error: {message}");
        }

        public static int GetDaysFromDate(DateTime fromDay) => Convert.ToInt32((DateTime.Now - fromDay).TotalDays);

        public static int GetDaysFromYear2022 => GetDaysFromDate(new DateTime(2022, 1, 1));
        public static int GetDaysFromYear2000 => GetDaysFromDate(new DateTime(2000, 1, 1));

        public static int ParseInt(string str) {
            try {
                return Int32.Parse(str);
            } catch (Exception) {
                return -1;
            }
        }
    }

    public enum NumberPattern {
        None,
        AutoInc,
        AutoDays,
        SetValue,
    }
}