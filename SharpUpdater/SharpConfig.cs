using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SharpUpdater {

    public class SharpConfig {
        public static string AppBase => System.AppContext.BaseDirectory;
        public const string DEFAULT_CONFIG = "SharpUpdater.json";

        public static SharpConfig? Read(string filename = DEFAULT_CONFIG) {
            var filepath = Path.Combine(AppBase, filename);
            if (!File.Exists(filepath)) {
                return default;
            }
            try {
                var content = File.ReadAllText(filepath, Encoding.UTF8);
                var sc = JsonConvert.DeserializeObject<SharpConfig>(content);
                Console.WriteLine($"ReadConfig file={filename} sc={sc}");
                return sc;
            } catch (Exception ex) {
                Console.WriteLine($"ReadConfig error={ex}");
                return default;
            }
        }

        public static bool Write(SharpConfig sc, string filename = DEFAULT_CONFIG) {
            var filepath = Path.Combine(AppBase, filename);
            try {
                var content = JsonConvert.SerializeObject(sc);
                Console.WriteLine($"WriteConfig file={filepath} content={content}");
                File.WriteAllText(filepath, content, Encoding.UTF8);
                return true;
            } catch (Exception ex) {
                Console.WriteLine($"WriteConfig error={ex}");
                return false;
            }
        }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string URL { get; set; } = string.Empty;

        public SharpConfig() {
        }

        public SharpConfig(string name, string uRL) {
            Name = name;
            URL = uRL;
        }

        public bool Malformed => string.IsNullOrEmpty(URL);

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}