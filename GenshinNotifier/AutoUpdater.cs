using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using CarrotCommon;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using Semver;

namespace GenshinNotifier {

    internal static class AutoUpdater {
        public static string ProjectUrl = "https://gitee.com/osap/CarrotProjects/tree/master/GenshinNotifier";
        public static string ReleaseUrl = "https://gitee.com/osap/CarrotProjects/releases";

        public static readonly List<string> VersionUrls = new List<string>(){
            "https://gitee.com/osap/CarrotProjects/raw/master/GenshinNotifier/version.json",
            "https://bitbucket.org/obitcat/carrotprojects/raw/master/GenshinNotifier/version.json",
            "https://raw.fastgit.org/mcxiaoke/CarrotProjects/master/GenshinNotifier/version.json",
            "https://raw.githubusercontent.com/mcxiaoke/CarrotProjects/master/GenshinNotifier/version.json"
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

        public static void ShowUpdater() {
            var updater = Path.Combine(Application.StartupPath, "SharpUpdater.exe");
            var name = Application.ProductName;
            var url = AutoUpdater.VersionUrls[0];
            if (File.Exists(updater)) {
                ProcessStartInfo startInfo = new ProcessStartInfo(updater) {
                    WindowStyle = ProcessWindowStyle.Normal,
                    Arguments = $"--name {name} --url \"{url}\""
                };
                Process.Start(startInfo);
            } else {
                Process.Start(AutoUpdater.ProjectUrl);
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
                        Logger.Info($"CheckUpdate info={info}");
                        if (info.HasUpdate) {
                            var newVersion = SemVersion.Parse(info.Version, SemVersionStyles.Any);
                            var oldVersion = SemVersion.Parse(Application.ProductVersion, SemVersionStyles.Any);
                            Logger.Debug($"CheckUpdate new={newVersion} old={oldVersion} hasNew={newVersion > oldVersion}");
                            HasNewVersion = oldVersion < newVersion;
                            CachedVersionInfo = info;
                            WriteUpdaterConfig(url);
                            if (HasNewVersion) {
                                CheckNotification(info);
                            }
                        }
                        return info;
                    } catch (Exception ex) {
                        Logger.Debug($"CheckUpdate failed error={ex.Message}");
                        return null;
                    }
                }
            });
        }

        private static void CheckNotification(VersionInfo info) {
            var settings = Properties.Settings.Default;
            if (!settings.OptionAutoUpdate) { return; }
            var savedNewVer = settings.NewVersionFound;
            var newVer = info.Version;
            if (newVer != savedNewVer) {
                settings.NewVersionFound = newVer;
                settings.Save();
                ShowNotification(info);
            }
        }

        private static void ShowNotification(VersionInfo info) {
            Logger.Debug($"ShowNotification new version={info.Version}");
            var image = AppUtils.IconFilePath;
            var toast = new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Reminder)
                .AddHeader($"2001", $"发现新版本 {info.Version}", "action=versionopen&id=2001")
                .AddText($"有新版本可以更新：{Application.ProductVersion} => {info.Version}")
                .AddText($"更新内容：{info.Changelog}")
                .AddAttributionText(DateTime.Now.ToString("F"))
                .AddAppLogoOverride(new Uri(image), ToastGenericAppLogoCrop.Circle)
                .AddButton(new ToastButton()
                .SetContent("立即更新")
                .AddArgument("action", "update")
                .SetBackgroundActivation());
            toast.Show(t => {
                t.Group = Application.ProductName;
                t.Tag = "VersionUpdate";
                t.ExpirationTime = DateTimeOffset.Now.AddHours(5);
            });
        }
    }
}