using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CarrotCommon;

namespace GenshinLib {
    public sealed class CacheManager {
        private string _name;
        public string Name {
            get => _name;
            set => _name = string.IsNullOrEmpty(value) ? "0" : value;
        }

        public CacheManager() : this("0") {
        }

        public CacheManager(string name) {
            this.Name = name;
        }

        public string GetCachePath(string key) {
            var root = Storage.UserDataFolder;
            var dir = Path.Combine(root, "cache", Name);
            // should be async
            Storage.CheckOrCreateDir(dir);
            var path = Path.Combine(dir, $"{key}.json");
            //Logger.Debug($"GetCachePath {path}");
            return path;
        }

        public async Task<T> LoadCache<T>(string key) {
            Logger.Debug($"LoadCache for {key}");
            return await Task.Run(() => {
                try {
                    var path = GetCachePath(key);
                    if (File.Exists(path)) {
                        var json = File.ReadAllText(path);
                        return JsonConvert.DeserializeObject<T>(json);
                    } else {
                        return default;
                    }
                } catch (Exception ex) {
                    Logger.Error($"LoadCache {key}", ex);
                    return default;
                }
            });
        }

        public async Task<T> LoadCache2<T>() {
            return await LoadCache<T>(typeof(T).Name);
        }

        public async Task SaveCache(string key, object data) {
            Logger.Debug($"SaveCache for {key}");
            await Task.Run(() => {
                try {
                    var path = GetCachePath(key);
                    var json = JsonConvert.SerializeObject(data);
                    File.WriteAllText(path, json);
                } catch (Exception ex) {
                    Logger.Error($"SaveCache {key}", ex);
                }
            });
        }

        public async Task SaveCache2(object data) {
            await SaveCache(data.GetType().Name, data);
        }
    }
}
