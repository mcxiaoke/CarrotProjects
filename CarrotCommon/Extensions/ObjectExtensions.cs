using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using CommunityToolkit.Diagnostics;

namespace Carrot.Common.Extensions;

internal static class ObjectExtCache<T> where T : new() {
    private static readonly Func<T, T> _cloner;

    static ObjectExtCache() {
        ParameterExpression param = Expression.Parameter(typeof(T), "in");

        var bindings = from prop in typeof(T).GetProperties()
                       where prop.CanRead && prop.CanWrite
                       select (MemberBinding)Expression.Bind(prop, Expression.Property(param, prop));

        _cloner = Expression.Lambda<Func<T, T>>(
            Expression.MemberInit(Expression.New(typeof(T)), bindings), param).Compile();
    }

    public static T Clone(T obj) {
        return _cloner(obj);
    }
}

/// <summary>
/// Provides extension methods for objects.
/// 提供对象的扩展方法。
/// </summary>
public static class ObjectExtensions {

    /// <summary>
    /// Creates a shallow copy of the object using reflection (via cached compiled expression trees).
    /// 使用反射（通过缓存的编译表达式树）创建对象的浅表副本。
    /// </summary>
    public static T CloneMe<T>(this T obj) where T : new() {
        return ObjectExtCache<T>.Clone(obj);
    }

    /// <summary>
    /// Creates a deep copy of the object using JSON serialization.
    /// 使用 JSON 序列化创建对象的深层副本。
    /// </summary>
    /// <typeparam name="T">The type of the object. 对象类型。</typeparam>
    /// <param name="value">The object to clone. 要克隆的对象。</param>
    /// <param name="settings">Optional JSON serializer settings. 可选的 JSON 序列化设置。</param>
    /// <returns>A new instance of T that is a deep copy of value. value 的深层副本。</returns>
    /// <exception cref="ArgumentNullException">Thrown if value is null. 如果 value 为 null 则抛出。</exception>
    public static T CloneJson<T>(this T value, JsonSerializerSettings? settings = null) where T : class {
        Guard.IsNotNull(value, nameof(value));
        string serialized = JsonConvert.SerializeObject(value, settings);
        return JsonConvert.DeserializeObject<T>(serialized, settings)!;
    }

    /// <summary>
    /// Returns a string representation of the object's public properties (Name: Value).
    /// 返回对象公共属性（名称：值）的字符串表示形式。
    /// </summary>
    public static string AsString(this object? obj) {
        if (obj == null) return string.Empty;
        return string.Join("\n",
            obj.GetType().GetProperties().Select(prop => $"{prop.Name}: {prop.GetValue(obj, null)}"));
    }
}
