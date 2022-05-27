using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenshinNotifier {
    public static class StringExtensions {
        public static string SafeSubstring(this string value, int startIndex, int length) {
            return new string((value ?? string.Empty).Skip(startIndex).Take(length).ToArray());
        }
    }


    public static class TimespanExtensions {
        public static string ToHumanReadableString(this TimeSpan timeSpan) {
            if (timeSpan.TotalSeconds >= 0 && timeSpan.TotalSeconds < 30) {
                return "刚刚";
            }
            var components = new List<Tuple<int, string>>
            {
            Tuple.Create(Math.Abs((int) timeSpan.TotalDays), "天"),
            Tuple.Create(Math.Abs(timeSpan.Hours), "小时"),
            Tuple.Create(Math.Abs(timeSpan.Minutes), "分钟"),
            Tuple.Create(Math.Abs(timeSpan.Seconds), "秒"),
        };
            components.RemoveAll(i => i.Item1 == 0);
            var timeStr = string.Join("", components.Select(t => $"{t.Item1}{t.Item2}"));
            string extra = timeSpan.TotalSeconds >= 0 ? "后" : "前";
            return $"{timeStr}{extra}";
        }
    }
}
