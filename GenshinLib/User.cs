using System;
using Newtonsoft.Json;

namespace GenshinLib {

    public class UserGameRole {

        [JsonProperty("game_biz")]
        public string GameBiz { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("game_uid")]
        public string GameUid { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("is_chosen")]
        public bool IsChosen { get; set; }

        [JsonProperty("region_name")]
        public string RegionName { get; set; }

        [JsonProperty("is_official")]
        public string IsOfficial { get; set; }

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