using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Carrot.Common;

/// <summary>
/// Provides general utility methods.
/// 提供通用实用程序方法。
/// </summary>
public static class Utility {
    
    /// <summary>
    /// Gets a random string representing a number between 100000 and 200000.
    /// 获取表示 100000 到 200000 之间数字的随机字符串。
    /// </summary>
    public static string GetRandomString2() {
        return Random.Shared.Next(100000, 200000).ToString();
    }

    /// <summary>
    /// Gets a random string of the specified length using alphanumeric characters.
    /// 使用字母数字字符获取指定长度的随机字符串。
    /// </summary>
    public static string GetRandomString(int length) {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Serializes an object to a JSON string.
    /// 将对象序列化为 JSON 字符串。
    /// </summary>
    public static string Stringify(object? value, bool indented = false) {
        if (value == null) { return ""; }
        var jst = new JsonSerializerSettings() {
            DateFormatString = "yyyy'-'MM'-'dd' 'HH':'mm':'ss.FFFFFFFK",
            Formatting = indented ? Formatting.Indented : Formatting.None,
        };
        return JsonConvert.SerializeObject(value, jst);
    }

    /// <summary>
    /// Validates if the cookie string contains required fields.
    /// 验证 cookie 字符串是否包含必填字段。
    /// </summary>
    public static bool ValiteCookieFields(string? value) {
        var cookieDict = ParseCookieString(value);
        var validKeys = new[] { "cookie_token", "login_ticket", "account_id" };
        return validKeys.Any(it => cookieDict.ContainsKey(it));
    }

    /// <summary>
    /// Parses a cookie string into a dictionary.
    /// 将 cookie 字符串解析为字典。
    /// </summary>
    public static Dictionary<string, string> ParseCookieString(string? str) {
        var cookieStr = str ?? string.Empty;
        var cookieDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(str)) { return cookieDictionary; }
        
        var values = cookieStr.TrimEnd(';').Split(';');
        foreach (var part in values) {
            var parts = part.Split(new[] { '=' }, 2);
            var cookieName = parts[0].Trim();
            string cookieValue = parts.Length > 1 ? parts[1] : string.Empty;
            
            if (!string.IsNullOrEmpty(cookieName)) {
                cookieDictionary[cookieName] = cookieValue;
            }
        }

        return cookieDictionary;
    }

    // Form data (for GET or POST) is usually encoded as application/x-www-form-urlencoded: this specifies + for spaces.
    // URLs are encoded as RFC 1738 which specifies %20.

    /// <summary>
    /// Creates a query string from a dictionary.
    /// 从字典创建查询字符串。
    /// </summary>
    public static string CreateQueryString(IDictionary<string, string>? dict) {
        if (dict == null || dict.Count == 0) { return string.Empty; }
        return string.Join("&", dict.OrderBy(x => x.Key).Select(kvp =>
            $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
    }

    /// <summary>
    /// Creates a query string from a dictionary (StringBuilder version).
    /// 从字典创建查询字符串（StringBuilder 版本）。
    /// </summary>
    public static string CreateQueryString2(IDictionary<string, string> dict) {
        if (dict == null || dict.Count == 0) { return string.Empty; }
        var sb = new StringBuilder();
        foreach (var kvp in dict.OrderBy(x => x.Key)) {
            sb.Append(Uri.EscapeDataString(kvp.Key))
              .Append('=')
              .Append(Uri.EscapeDataString(kvp.Value))
              .Append('&');
        }
        if (sb.Length > 0) {
            sb.Length--;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Parses a query string into a dictionary.
    /// 将查询字符串解析为字典。
    /// </summary>
    public static Dictionary<string, string> ParseQueryString(string query) {
        var queryDict = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(query)) return queryDict;

        var tokens = query.TrimStart('?').Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens) {
            var parts = token.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries); // limited split to 2
            // Note: Original code logic used EscapeDataString which seems wrong for parsing. 
            // Switched to UnescapeDataString for correctness.
            // Also relaxed split to allow empty values.
            // However, keeping closer to original structure but safer.
            
            if (parts.Length > 0) {
                string key = parts[0].Trim();
                string value = parts.Length == 2 ? Uri.UnescapeDataString(parts[1]).Trim() : "";
                queryDict[key] = value;
            }
        }
        return queryDict;
    }

    /// <summary>
    /// Verifies if the hash of input matches the provided hash.
    /// 验证输入的哈希值是否与提供的哈希值匹配。
    /// </summary>
    public static bool VerifyHash(HashAlgorithm hashAlgorithm, string input, string hash) {
        var hashOfInput = GetHash(hashAlgorithm, input);
        return StringComparer.OrdinalIgnoreCase.Compare(hashOfInput, hash) == 0;
    }

    /// <summary>
    /// Computes the hash of the input string using the specified algorithm.
    /// 使用指定的算法计算输入字符串的哈希值。
    /// </summary>
    public static string GetHash(HashAlgorithm hashAlgorithm, string input) {
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(data).ToLowerInvariant();
    }

    /// <summary>
    /// Computes the MD5 hash of the input string.
    /// 计算输入字符串的 MD5 哈希值。
    /// </summary>
    public static string GetMD5Hash(string input) {
        using var md5 = MD5.Create();
        return GetHash(md5, input);
    }

    /// <summary>
    /// Computes the MD5 hash of the source string (Duplicate of GetMD5Hash).
    /// 计算源字符串的 MD5 哈希值（GetMD5Hash 的副本）。
    /// </summary>
    public static string GetComputedMd5(string source) {
        return GetMD5Hash(source);
    }
}