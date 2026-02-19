using System;
using System.Runtime.Caching;

namespace Carrot.Common;

/// <summary>
/// Provides a simple in-memory cache store.
/// 提供简单的内存缓存存储。
/// </summary>
public static class CacheStore {
    private static readonly ObjectCache _cache = MemoryCache.Default;

    /// <summary>
    /// Adds an item to the cache with the default policy (infinite expiration).
    /// 使用默认策略（无限过期）将项添加到缓存中。
    /// </summary>
    /// <param name="key">The cache key. 缓存键。</param>
    /// <param name="value">The object to cache. 要缓存的对象。</param>
    /// <returns>True if insertion succeeded, or false if there is an already an entry with the same key. 如果插入成功则为 True，如果已存在具有相同键的条目则为 False。</returns>
    public static bool Add(string key, object value) {
        return _cache.Add(key, value, null);
    }

    /// <summary>
    /// Adds an item to the cache with an absolute expiration time.
    /// 将具有绝对过期时间的项添加到缓存中。
    /// </summary>
    /// <param name="key">The cache key. 缓存键。</param>
    /// <param name="value">The object to cache. 要缓存的对象。</param>
    /// <param name="expireIn">The absolute expiration time. 绝对过期时间。</param>
    /// <returns>True if insertion succeeded, or false if there is an already an entry with the same key. 如果插入成功则为 True，如果已存在具有相同键的条目则为 False。</returns>
    public static bool Add(string key, object value, DateTimeOffset expireIn) {
        return _cache.Add(key, value, expireIn);
    }

    /// <summary>
    /// Gets an item from the cache.
    /// 从缓存中获取项。
    /// </summary>
    /// <param name="key">The cache key. 缓存键。</param>
    /// <returns>The cached object, or null if the key was not found. 缓存的对象，如果未找到键，则为 null。</returns>
    public static object? Get(string key) {
        return _cache.Get(key);
    }

    /// <summary>
    /// Removes an item from the cache.
    /// 从缓存中移除项。
    /// </summary>
    /// <param name="key">The cache key. 缓存键。</param>
    /// <returns>The removed object, or null if the key was not found. 已移除的对象，如果未找到键，则为 null。</returns>
    public static object? Remove(string key) {
        return _cache.Remove(key);
    }
}