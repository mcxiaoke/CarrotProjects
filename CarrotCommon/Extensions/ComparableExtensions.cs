using System;
using System.Globalization;

namespace Carrot.Common.Extensions;

/// <summary>
/// Provides extension methods for comparable types.
/// 提供可比较类型的扩展方法。
/// </summary>
public static class ComparableExtensions {

    /// <summary>
    /// Constrains <paramref name="value" /> to fall within the range [<paramref name="min" />, <paramref name="max" />].
    /// If <paramref name="value" /> is less than <paramref name="min" />, <paramref name="min" /> is returned.
    /// If <paramref name="value" /> is greater than <paramref name="max" />, <paramref name="max" /> is returned.
    /// Otherwise, <paramref name="value"/> is returned.
    /// 将 <paramref name="value" /> 限制在 [<paramref name="min" />, <paramref name="max" />] 范围内。
    /// 如果 <paramref name="value" /> 小于 <paramref name="min" />，则返回 <paramref name="min" />。
    /// 如果 <paramref name="value" /> 大于 <paramref name="max" />，则返回 <paramref name="max" />。
    /// 否则，返回 <paramref name="value"/>。
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="value"/>. <paramref name="value"/> 的类型。</typeparam>
    /// <param name="value">The value to constrain. 要限制的值。</param>
    /// <param name="min">The lower bound (inclusive). 下限（包含）。</param>
    /// <param name="max">The upper bound (inclusive). 上限（包含）。</param>
    /// <returns>The constrained value. 限制后的值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="min"/> is greater than <paramref name="max"/>. 如果 <paramref name="min"/> 大于 <paramref name="max"/> 则抛出。</exception>
    public static T ConstrainToRange<T>(this T value, T min, T max) where T : IComparable<T> {
        if (min.CompareTo(max) > 0) {
            var minString = Convert.ToString(min, CultureInfo.InvariantCulture);
            var maxString = Convert.ToString(max, CultureInfo.InvariantCulture);
            throw new ArgumentOutOfRangeException(nameof(min), $"The argument {nameof(min)} ({minString}) must not be greater than the argument {nameof(max)} ({maxString}).");
        }

        // Optimization: fewer comparisons
        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }
}