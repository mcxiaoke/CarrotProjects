using System.IO;

namespace Carrot.Common;

/// <summary>
/// Provides local storage utilities.
/// 提供本地存储实用程序。
/// </summary>
public static class Storage {
    /// <summary>
    /// Checks if the directory exists and creates it if it doesn't.
    /// 检查目录是否存在，如果不存在则创建它。
    /// </summary>
    /// <param name="path">The directory path. 目录路径。</param>
    public static void CheckOrCreateDir(string path) {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }
}