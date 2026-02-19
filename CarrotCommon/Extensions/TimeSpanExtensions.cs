using System;
using System.Collections.Generic;
using System.Linq;

namespace Carrot.Common.Extensions;

/// <summary>
/// Provides extension methods for TimeSpan.
/// 提供 TimeSpan 的扩展方法。
/// </summary>
public static class TimeSpanExtensions {

    /// <summary>
    /// Converts a TimeSpan to a human-readable string (e.g., "5分前", "2小时后", "刚刚").
    /// 将 TimeSpan 转换为人类可读的字符串（例如，“5分前”，“2小时后”，“刚刚”）。
    /// </summary>
    /// <param name="timeSpan">The time span to convert. 要转换的时间跨度。</param>
    /// <returns>A human-readable string. 人类可读的字符串。</returns>
    public static string ToHumanReadableString(this TimeSpan timeSpan) {
        if (timeSpan.TotalSeconds >= 0 && timeSpan.TotalSeconds < 30) {
            return "刚刚"; // Just now
        }

        var components = new List<(int Value, string Unit)> {
            ((int)Math.Abs(timeSpan.TotalDays), "天"),
            (Math.Abs(timeSpan.Hours), "小时"),
            (Math.Abs(timeSpan.Minutes), "分"),
            (Math.Abs(timeSpan.Seconds), "秒")
        };

        // Remove components with 0 value
        components.RemoveAll(i => i.Value == 0);

        string timeStr = string.Concat(components.Select(t => $"{t.Value}{t.Unit}"));
        string extra = timeSpan.TotalSeconds >= 0 ? "后" : "前"; // After / Before (Ago)

        return $"{timeStr}{extra}";
    }
}
