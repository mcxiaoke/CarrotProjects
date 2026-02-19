using System.Linq;

namespace Carrot.Common.Extensions;

/// <summary>
/// Provides extension methods for strings.
/// 提供字符串的扩展方法。
/// </summary>
public static class StringExtensions {

    /// <summary>
    /// Safely retrieves a substring from this instance.
    /// 从此实例安全地检索子字符串。
    /// </summary>
    /// <param name="value">The string to retrieve the substring from. 要检索子字符串的字符串。</param>
    /// <param name="startIndex">The zero-based starting character position of a substring as an integer. 子字符串的从零开始的起始字符位置。</param>
    /// <param name="length">The number of characters in the substring. 子字符串中的字符数。</param>
    /// <returns>A string that is equivalent to the substring of length length that begins at startIndex in this instance, or Empty if startIndex is out of range. 相当于从此实例中的 startIndex 处开始的长度为 length 的子字符串的字符串，如果 startIndex 超出范围，则为空。</returns>
    public static string SafeSubstring(this string? value, int startIndex, int length) {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        if (startIndex < 0) startIndex = 0;
        if (length < 0) return string.Empty;

        // Optimization: Use Span for better performance
        // return value.AsSpan().Slice(start, length).ToString();
        // But need bounds checking logic from Linq Skip/Take
        
        return new string(value.Skip(startIndex).Take(length).ToArray());
    }
}
