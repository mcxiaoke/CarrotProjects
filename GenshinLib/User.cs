using System;
using Newtonsoft.Json;

namespace GenshinLib {

    public class UserGameRole {

        [JsonProperty("game_biz")]
        public string GameBiz { get; set; } = string.Empty;

        [JsonProperty("region")]
        public string Region { get; set; } = string.Empty;

        [JsonProperty("game_uid")]
        public string GameUid { get; set; } = string.Empty;

        [JsonProperty("nickname")]
        public string Nickname { get; set; } = string.Empty;

        [JsonProperty("level")]
        public int Level { get; set; } = 1;

        [JsonProperty("is_chosen")]
        public bool IsChosen { get; set; }

        [JsonProperty("region_name")]
        public string RegionName { get; set; } = string.Empty;

        [JsonProperty("is_official")]
        public string IsOfficial { get; set; } = string.Empty;

        /// <summary>
        /// 数据更新时间
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        public string Server => GameBiz == "hk4e_cn" ? "CN" : "OS";

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}