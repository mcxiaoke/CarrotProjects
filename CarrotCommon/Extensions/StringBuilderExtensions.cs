using System;
using System.Text;

namespace Carrot.Common.Extensions;

/// <summary>
/// Provides extension methods for StringBuilder.
/// 提供 StringBuilder 的扩展方法。
/// </summary>
public static class StringBuilderExtensions {

    /// <summary>
    /// Appends the string representation of all values that satisfy the predicate.
    /// 追加所有满足条件的字符串表示形式。
    /// </summary>
    /// <typeparam name="T">The type of the values. 值的类型。</typeparam>
    /// <param name="this">The StringBuilder to append to. 要追加到的 StringBuilder。</param>
    /// <param name="predicate">A function to test each element for a condition. 测试每个元素的条件。</param>
    /// <param name="values">The values to append. 要追加的值。</param>
    /// <returns>A reference to this instance after the append operation has completed. 追加操作完成后对该实例的引用。</returns>
    public static StringBuilder AppendIf<T>(this StringBuilder @this, Func<T, bool> predicate, params T[] values) {
        if (@this == null) throw new ArgumentNullException(nameof(@this));
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        
        foreach (var value in values) {
            if (predicate(value)) {
                @this.Append(value);
            }
        }
        return @this;
    }

    /// <summary>
    /// Appends the value if the condition is true.
    /// 如果条件为 true，则追加值。
    /// </summary>
    /// <param name="this">The StringBuilder to append to. 要追加到的 StringBuilder。</param>
    /// <param name="condition">The condition to evaluate. 要评估的条件。</param>
    /// <param name="value">The string to append. 要追加的字符串。</param>
    /// <returns>A reference to this instance after the append operation has completed. 追加操作完成后对该实例的引用。</returns>
    public static StringBuilder AppendIf(this StringBuilder @this, bool condition, string value) {
        if (@this == null) throw new ArgumentNullException(nameof(@this));
        if (condition) {
            @this.Append(value);
        }
        return @this;
    }
}
