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
using CarrotCommon;
using GenshinLib;
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
                            var oldVersion = SemVersion.Parse(Application.ProductVersion, SemVersionStyles.Any);
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

}
