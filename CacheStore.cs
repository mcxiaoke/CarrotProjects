using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace GenshinNotifier {
    internal class CacheStore {
        private static ObjectCache _cache = MemoryCache.Default;

        public static bool Add(string key, object value) {
            return _cache.Add(key, value, null);
        }

        public static bool Add(string key, object value, DateTimeOffset expireIn) {
            return _cache.Add(key, value, expireIn);
        }

        public static object Get(string key) {
            return _cache.Get(key, null);
        }

        public static object Remove(string key) {
            return _cache.Remove(key);
        }
    }
}
