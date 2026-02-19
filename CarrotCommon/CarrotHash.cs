using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Carrot.Common;

/// <summary>
/// Provides hashing utilities.
/// 提供哈希实用程序。
/// </summary>
public static class CarrotHash {
    /// <summary>
    /// Converts a byte array to a hex string.
    /// 将字节数组转换为十六进制字符串。
    /// </summary>
    /// <param name="data">The byte array to convert. 要转换的字节数组。</param>
    /// <param name="toUpper">Whether to use uppercase hex characters. 是否使用大写十六进制字符。</param>
    /// <param name="grouping">Whether to add spaces for grouping. 是否添加空格进行分组。</param>
    /// <returns>The hex string. 十六进制字符串。</returns>
    public static string ToHexString(byte[] data, bool toUpper = false, bool grouping = false) {
        if (data == null || data.Length == 0) {
            return string.Empty;
        }

        if (!grouping) {
            string hex = Convert.ToHexString(data);
            return toUpper ? hex : hex.ToLowerInvariant();
        }

        var sb = new StringBuilder(data.Length * 3);
        for (int i = 0; i < data.Length; i++) {
            sb.Append(data[i].ToString("x2"));
            if ((i % 4) == 3) {
                sb.Append(' ');
            }
        }
        return toUpper ? sb.ToString().ToUpperInvariant() : sb.ToString();
    }

    /// <summary>
    /// Computes the hash of a file using the specified algorithm.
    /// 使用指定的算法计算文件的哈希值。
    /// </summary>
    /// <param name="filepath">The path to the file. 文件路径。</param>
    /// <param name="hasher">The hash algorithm to use. 要使用的哈希算法。</param>
    /// <param name="toUpper">Whether to output uppercase hex. 是否输出大写十六进制。</param>
    /// <returns>The hash string. 哈希字符串。</returns>
    public static string FileHash(string filepath, HashAlgorithm hasher, bool toUpper = false) {
        if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath)) { 
            return string.Empty; 
        }

        try {
            using var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] hashValue = hasher.ComputeHash(stream);
            return ToHexString(hashValue, toUpper);
        } catch (IOException e) {
            Logger.Warning($"GetHash I/O Exception: {e.Message}");
        } catch (UnauthorizedAccessException e) {
            Logger.Warning($"GetHash Access Exception: {e.Message}");
        } catch (Exception e) {
             Logger.Warning($"GetHash Exception: {e.Message}");
        }
        return string.Empty;
    }

    /// <summary>
    /// Computes the MD5 hash of a file.
    /// 计算文件的 MD5 哈希值。
    /// </summary>
    public static string FileMD5(string filepath, bool toUpper = false) {
        using var hasher = MD5.Create();
        return FileHash(filepath, hasher, toUpper);
    }

    /// <summary>
    /// Computes the SHA1 hash of a file.
    /// 计算文件的 SHA1 哈希值。
    /// </summary>
    public static string FileSHA1(string filepath, bool toUpper = false) {
        using var hasher = SHA1.Create();
        return FileHash(filepath, hasher, toUpper);
    }

    /// <summary>
    /// Computes the SHA256 hash of a file.
    /// 计算文件的 SHA256 哈希值。
    /// </summary>
    public static string FileSHA256(string filepath, bool toUpper = false) {
        using var hasher = SHA256.Create();
        return FileHash(filepath, hasher, toUpper);
    }

    /// <summary>
    /// Computes the SHA384 hash of a file.
    /// 计算文件的 SHA384 哈希值。
    /// </summary>
    public static string FileSHA384(string filepath, bool toUpper = false) {
        using var hasher = SHA384.Create();
        return FileHash(filepath, hasher, toUpper);
    }

    /// <summary>
    /// Computes the SHA512 hash of a file.
    /// 计算文件的 SHA512 哈希值。
    /// </summary>
    public static string FileSHA512(string filepath, bool toUpper = false) {
        using var hasher = SHA512.Create();
        return FileHash(filepath, hasher, toUpper);
    }
}