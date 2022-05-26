using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenshinNotifier.Properties;

namespace GenshinNotifier {
    public static class AppUtils {

        public static int ThreadId => System.Threading.Thread.CurrentThread.ManagedThreadId;

        public const string ICON_FILE_NAME = "carrot_512.png";
        public static readonly string IconFilePath = Path.Combine(Storage.UserDataFolder, "assets", ICON_FILE_NAME);
        public static async Task CheckLocalAssets() {
            await Task.Run(() => {
                var assetsDir = Directory.GetParent(IconFilePath).FullName;
                Storage.CheckOrCreateDir(assetsDir);
                if (!File.Exists(IconFilePath)) {
                    Resources.ImageCarrot512.Save(IconFilePath);
                    Logger.Info($"CheckLocalAssets copied to {IconFilePath}");
                }
            });
        }
    }
}
