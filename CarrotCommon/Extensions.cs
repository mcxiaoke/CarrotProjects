using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CarrotCommon {

    public static class CarrotExtensions {

        public static string AsString(this object obj) => string.Join("\n",
            obj.GetType().GetProperties().Select(prop => $"{prop.Name}: {prop.GetValue(obj, null)}"));

        public static string ToHumanReadableString(this TimeSpan timeSpan) {
            if (timeSpan.TotalSeconds >= 0 && timeSpan.TotalSeconds < 30) {
                return "刚刚";
            }
            var components = new List<Tuple<int, string>>
            {
            Tuple.Create(Math.Abs((int) timeSpan.TotalDays), "天"),
            Tuple.Create(Math.Abs(timeSpan.Hours), "小时"),
            Tuple.Create(Math.Abs(timeSpan.Minutes), "分"),
            Tuple.Create(Math.Abs(timeSpan.Seconds), "秒"),
        };
            components.RemoveAll(i => i.Item1 == 0);
            var timeStr = string.Concat(components.Select(t => $"{t.Item1}{t.Item2}"));
            string extra = timeSpan.TotalSeconds >= 0 ? "后" : "前";
            return $"{timeStr}{extra}";
        }

        public static string SafeSubstring(this string value, int startIndex, int length) {
            return new string((value ?? string.Empty).Skip(startIndex).Take(length).ToArray());
        }

        public static StringBuilder AppendIf<T>(this StringBuilder @this, Func<T, bool> predicate, params T[] values) {
            foreach (var value in values) {
                if (predicate(value)) {
                    @this.Append(value);
                }
            }

            return @this;
        }

        public static StringBuilder AppendIf(this StringBuilder @this, bool condition, string value) {
            if (condition)
                @this.Append(value);
            return @this;
        }
    }
}