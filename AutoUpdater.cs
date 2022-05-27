using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Semver;
using GenshinNotifier.Net;
using System.Security.Policy;

namespace GenshinNotifier {


    static class AutoUpdater {


        public static string ProjectUrl = "https://gitee.com/osap/GenshinNotifier";

        public static readonly List<string> VersionUrls = new List<string>(){
            "https://gitee.com/osap/GenshinNotifier/raw/master/version.json",
            "https://bitbucket.org/obitcat/carrotnotifier/raw/master/version.json",
            "https://raw.fastgit.org/mcxiaoke/CarrotNotifier/master/version.json",
            "https://fastly.jsdelivr.net/gh/mcxiaoke/CarrotNotifier@master/version.json",
            "https://raw.githubusercontent.com/mcxiaoke/CarrotNotifier/master/version.json"
        };

        public static string UpdaterConfigFileName = "SharpUpdater.json";

        public static bool HasNewVersion;
        public static VersionInfo CachedVersionInfo;

        private static void WriteUpdaterConfig(string url) {
            var config = Path.Combine(Application.StartupPath, UpdaterConfigFileName);
            try {
                if (!File.Exists(config)) {
                    var obj = new {
                        name = Application.ProductName, url
                    };
                    Logger.Info($"WriteUpdaterConfigg config={obj.name} {obj.url}");
                    File.WriteAllText(config, JsonConvert.SerializeObject(obj));
                }
            } catch (Exception ex) {
                Logger.Debug($"WriteUpdaterConfig failed={ex.Message}");
            }
        }

        private static (string, string) ReadUpdaterConfig() {
            var config = Path.Combine(Application.StartupPath, UpdaterConfigFileName);
            try {
                dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(config));
                Logger.Info($"ReadUpdaterConfig config={obj.name} {obj.url}");
                return (obj.name, obj.url);
            } catch (Exception ex) {
                Logger.Debug($"ReadUpdaterConfig failed={ex.Message}");
                return (null, null);
            }
        }

        public static async Task<VersionInfo> CheckUpdate() {
            return await Task.Run(async () => {
                var url = VersionUrls[0];
                using (var client = new WebClient()) {
                    Logger.Debug($"CheckUpdate info url={url}");
                    try {
                        var text = await client.DownloadStringTaskAsync(new Uri(url));
                        var info = JsonConvert.DeserializeObject<VersionInfo>(text);
                        Logger.Debug($"CheckUpdate info={info}");
                        if (info.HasUpdate) {
                            var newVersion = SemVersion.Parse(info.Version, SemVersionStyles.Any);
                            var oldVersion = SemVersion.Parse(Storage.AppVersion, SemVersionStyles.Any);
                            Logger.Debug($"CheckUpdate new={newVersion} old={oldVersion} hasNew={newVersion > oldVersion}");
                            HasNewVersion = oldVersion < newVersion;
                            CachedVersionInfo = info;
                            WriteUpdaterConfig(url);
                        }
                        return info;
                    } catch (Exception ex) {
                        Logger.Debug($"CheckUpdate failed error={ex.Message}");
                        return null;
                    }
                }
            });
        }

    }

    class VersionInfo {
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
