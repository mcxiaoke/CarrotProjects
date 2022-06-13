using System.IO;

namespace CarrotCommon {

    public static class Storage {
        private static readonly object _SyncObj = new object();

        public static void CheckOrCreateDir(string path) {
            lock (_SyncObj) {
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
            }
        }
    }
}