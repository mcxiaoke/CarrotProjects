using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenshinNotifier.Net;
using Newtonsoft.Json;

namespace GenshinNotifier {
    public class CacheManager {
        private string _name;
        public string Name {
            get => _name;
            set => _name = String.IsNullOrEmpty(value) ? "default" : value;
        }

        public CacheManager() : this("default") {
        }

        public CacheManager(string name) {
            this.Name = name;
        }

        public string GetCachePath(string key) {
            var root = Storage.UserDataFolder;
            var dir = Path.Combine(root, "Cache", Name);
            // should be async
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            var path = Path.Combine(dir, $"{key}.json");
            Logger.Debug($"GetCachePath {path}");
            return path;
        }

        public async Task<T> LoadCache<T>(string key) {
            Logger.Debug($"LoadCache for {key}");
            try {
                var path = GetCachePath(key);
                if (!File.Exists(path)) {
                    return default;
                }
                var json = await Task.Run(() => File.ReadAllText(path));
                return JsonConvert.DeserializeObject<T>(json);
            } catch (Exception ex) {
                Logger.Error($"LoadCache {key} error={ex.Message}");
                return default;
            }
        }

        public async Task<T> LoadCache2<T>() {
            var key = typeof(T).Name;
            Logger.Debug($"LoadCache2 for {key}");
            try {
                var path = GetCachePath(key);
                if (!File.Exists(path)) {
                    return default;
                }
                var json = await Task.Run(() => File.ReadAllText(path));
                return JsonConvert.DeserializeObject<T>(json);
            } catch (Exception ex) {
                Logger.Error($"LoadCache {key} error={ex.Message}");
                return default;
            }
        }

        public async Task SaveCache(string key, object data) {
            Logger.Debug($"SaveCache for {key}");
            try {
                var path = GetCachePath(key);
                var json = JsonConvert.SerializeObject(data);
                await Task.Run(() => File.WriteAllText(path, json));
            } catch (Exception ex) {
                Logger.Error($"SaveCache {key} error={ex.Message}");

            }
        }

        public async Task SaveCache2(object data) {
            var key = data.GetType().Name;
            Logger.Debug($"SaveCache for {key}");
            try {
                var path = GetCachePath(key);
                var json = JsonConvert.SerializeObject(data);
                await Task.Run(() => File.WriteAllText(path, json));
            } catch (Exception ex) {
                Logger.Error($"SaveCache {key} error={ex.Message}");

            }
        }
    }
}
