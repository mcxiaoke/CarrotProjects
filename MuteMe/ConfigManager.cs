using System;
using System.IO;
using System.Text.Json;

namespace MuteMe {
    public static class ConfigManager {
        private static readonly string ConfigDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MuteMe");

        private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

        public static AppConfig Load() {
            try {
                if (!File.Exists(ConfigFile)) {
                    return new AppConfig();
                }

                var json = File.ReadAllText(ConfigFile);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            } catch {
                return new AppConfig();
            }
        }

        public static void Save(AppConfig config) {
            try {
                Directory.CreateDirectory(ConfigDir);
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions {
                    WriteIndented = true
                });
                File.WriteAllText(ConfigFile, json);
            } catch {
            }
        }
    }
}
