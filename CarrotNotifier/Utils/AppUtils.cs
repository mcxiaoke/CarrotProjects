using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Carrot.Common;
using GenshinNotifier.Properties;

namespace GenshinNotifier {

    public static class AppUtils {
        public static int ThreadId => System.Threading.Thread.CurrentThread.ManagedThreadId;

        public const string ICON_FILE_NAME = "carrot_512.png";
        public static readonly string IconFilePath = Path.Combine(AppInfo.LocalAppDataPath, "assets", ICON_FILE_NAME);

        public static async Task CheckLocalAssets() {
            await Task.Run(() => {
                var assetsDir = Directory.GetParent(IconFilePath)?.FullName;
                if (assetsDir != null) {
                    Storage.CheckOrCreateDir(assetsDir);
                    if (!File.Exists(IconFilePath)) {
                        Resources.ImageCarrot512.Save(IconFilePath);
                        Logger.Info($"CheckLocalAssets copied to {IconFilePath}");
                    }
                }
            });
        }

        public static bool PropertyExists(dynamic obj, string name) {
            if (obj == null)
                return false;
            if (obj is IDictionary<string, object> dict) {
                return dict.ContainsKey(name);
            }
            if (obj is Newtonsoft.Json.Linq.JObject jobj) {
                return jobj.ContainsKey(name);
            }
            return obj.GetType().GetProperty(name) != null;
        }
    }
}