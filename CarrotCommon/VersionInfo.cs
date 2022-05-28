using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CarrotCommon {
    public class VersionInfo {
        [JsonProperty("has_update")]
        public bool HasUpdate { get; set; }

        [JsonProperty("release")]
        public bool Release { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("program")]
        public string Program { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("changelog")]
        public string Changelog { get; set; }

        [JsonProperty("sha256sum")]
        public string Sha256sum { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("new_url")]
        public string NewUrl { get; set; }

        [JsonProperty("project_url")]
        public string ProjectUrl { get; set; }

        [JsonProperty("download_size")]
        public int DownloadSize { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }

        [JsonProperty("updater_url")]
        public string UpdaterUrl { get; set; }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }

        [JsonIgnore]
        public string LocalName { get; set; }

        [JsonIgnore]
        public string LocalVersion { get; set; }

        public static bool DataInValid(VersionInfo info) =>
            info == null
            || string.IsNullOrWhiteSpace(info.Version)
            || string.IsNullOrWhiteSpace(info.DownloadUrl)
            || string.IsNullOrWhiteSpace(info.Program);
    }
}
