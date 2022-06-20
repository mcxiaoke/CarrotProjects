using System;
using Newtonsoft.Json;

namespace Carrot.Common {

    public class VersionInfo {

        [JsonProperty("has_update")]
        public bool HasUpdate { get; set; } = false;

        [JsonProperty("release")]
        public bool Release { get; set; } = false;

        [JsonProperty("id")]
        public int Id { get; set; } = 0;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("program")]
        public string Program { get; set; } = string.Empty;

        [JsonProperty("version")]
        public string Version { get; set; } = string.Empty;

        [JsonProperty("changelog")]
        public string Changelog { get; set; } = string.Empty;

        [JsonProperty("sha256sum")]
        public string Sha256sum { get; set; } = string.Empty;

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.MinValue;

        [JsonProperty("new_url")]
        public string NewUrl { get; set; } = string.Empty;

        [JsonProperty("project_url")]
        public string ProjectUrl { get; set; } = string.Empty;

        [JsonProperty("download_size")]
        public int DownloadSize { get; set; } = 0;

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; } = string.Empty;

        [JsonProperty("updater_url")]
        public string UpdaterUrl { get; set; } = string.Empty;

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        [JsonIgnore]
        public string LocalName { get; set; } = string.Empty;

        [JsonIgnore]
        public string LocalVersion { get; set; } = string.Empty;

        public static bool DataInValid(VersionInfo info) =>
            info == null
            || string.IsNullOrWhiteSpace(info.Version)
            || string.IsNullOrWhiteSpace(info.DownloadUrl)
            || string.IsNullOrWhiteSpace(info.Program);
    }
}