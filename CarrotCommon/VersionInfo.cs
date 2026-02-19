using System;
using Newtonsoft.Json;

namespace Carrot.Common;

/// <summary>
/// Represents application version information.
/// 表示应用程序版本信息。
/// </summary>
public class VersionInfo {

    /// <summary>
    /// Indicates whether a new update is available.
    /// 指示是否有新的更新可用。
    /// </summary>
    [JsonProperty("has_update")]
    public bool HasUpdate { get; set; }

    /// <summary>
    /// Indicates whether this is a release version.
    /// 指示这是否是发布版本。
    /// </summary>
    [JsonProperty("release")]
    public bool Release { get; set; }

    /// <summary>
    /// The unique identifier of the version.
    /// 版本的唯一标识符。
    /// </summary>
    [JsonProperty("id")]
    public int Id { get; set; }

    /// <summary>
    /// The name of the version.
    /// 版本名称。
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The program name associated with this version.
    /// 与此版本关联的程序名称。
    /// </summary>
    [JsonProperty("program")]
    public string Program { get; set; } = string.Empty;

    /// <summary>
    /// The version string (e.g., "1.0.0").
    /// 版本字符串（例如 "1.0.0"）。
    /// </summary>
    [JsonProperty("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The changelog for this version.
    /// 此版本的更改日志。
    /// </summary>
    [JsonProperty("changelog")]
    public string Changelog { get; set; } = string.Empty;

    /// <summary>
    /// The SHA256 checksum of the update file.
    /// 更新文件的 SHA256 校验和。
    /// </summary>
    [JsonProperty("sha256sum")]
    public string Sha256sum { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the version was created.
    /// 创建版本的日期和时间。
    /// </summary>
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The URL for the new version information or page.
    /// 新版本信息或页面的 URL。
    /// </summary>
    [JsonProperty("new_url")]
    public string NewUrl { get; set; } = string.Empty;

    /// <summary>
    /// The project homepage URL.
    /// 项目主页 URL。
    /// </summary>
    [JsonProperty("project_url")]
    public string ProjectUrl { get; set; } = string.Empty;

    /// <summary>
    /// The size of the download file in bytes.
    /// 下载文件的大小（字节）。
    /// </summary>
    [JsonProperty("download_size")]
    public long DownloadSize { get; set; }

    /// <summary>
    /// The direct download URL for the update file.
    /// 更新文件的直接下载 URL。
    /// </summary>
    [JsonProperty("download_url")]
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// The URL for the updater executable or package.
    /// 更新程序可执行文件或包的 URL。
    /// </summary>
    [JsonProperty("updater_url")]
    public string UpdaterUrl { get; set; } = string.Empty;

    /// <summary>
    /// Local version name (runtime only).
    /// 本地版本名称（仅运行时）。
    /// </summary>
    [JsonIgnore]
    public string LocalName { get; set; } = string.Empty;

    /// <summary>
    /// Local version string (runtime only).
    /// 本地版本字符串（仅运行时）。
    /// </summary>
    [JsonIgnore]
    public string LocalVersion { get; set; } = string.Empty;

    public override string ToString() {
        return Utility.Stringify(this, true);
    }

    /// <summary>
    /// Checks if the version info is invalid.
    /// 检查版本信息是否无效。
    /// </summary>
    public static bool DataInValid(VersionInfo? info) {
        if (info == null) return true;
        return string.IsNullOrWhiteSpace(info.Version) ||
               string.IsNullOrWhiteSpace(info.DownloadUrl);
    }
}