using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GenshinLib {

    public enum KnownReturnCode {
        InternalFailure = int.MinValue,
        AlreadySignedIn = -5003,
        AuthKeyTimeOut = -101,
        OK = 0,
        NotDefined = 7,
        DataIsNotPublicForTheUser = 10102,
    }

    /// <summary>
    /// 提供 <see cref="Response{T}"/> 的非泛型基类
    /// </summary>
    public class Response {
        /// <summary>
        /// 0 is OK
        /// </summary>
        [JsonProperty("retcode")] public int ReturnCode { get; set; }
        [JsonProperty("message")] public string? Message { get; set; }

        public override string ToString() {
            return $"状态：{ReturnCode} | 信息：{Message}";
        }

        public static bool IsOk(Response? response) {
            return response?.ReturnCode == 0;
        }
        public static Response CreateFail(string message) {
            return new Response() {
                ReturnCode = (int)KnownReturnCode.InternalFailure,
                Message = message
            };
        }
    }

    /// <summary>
    /// Mihoyo 标准API响应
    /// </summary>
    /// <typeparam name="TData">数据类型</typeparam>
    public class Response<TData> : Response {
        [JsonProperty("data")] public TData? Data { get; set; }
        public static new Response<TData> CreateFail(string message) {
            return new Response<TData>() {
                ReturnCode = (int)KnownReturnCode.InternalFailure,
                Message = message
            };
        }
    }

    public class SignInResult {
        /// <summary>
        /// 通常是 ""
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 通常是 0
        /// </summary>
        [JsonProperty("risk_code")]
        public int RiskCode { get; set; }

        /// <summary>
        /// 通常是 ""
        /// </summary>
        [JsonProperty("gt")]
        public string Gt { get; set; } = string.Empty;

        /// <summary>
        /// 通常是 ""
        /// </summary>
        [JsonProperty("challenge")]
        public string Challenge { get; set; } = string.Empty;

        /// <summary>
        /// 通常是 1
        /// </summary>
        [JsonProperty("success")]
        public int Success { get; set; }
    }

    public class SignInAward {
        [JsonProperty("icon")] public string? Icon { get; set; }
        [JsonProperty("name")] public string? Name { get; set; }
        [JsonProperty("cnt")] public string? Count { get; set; }
    }

    public class SignInReward {
        /// <summary>
        /// 月份
        /// </summary>
        [JsonProperty("month")] public string? Month { get; set; }
        [JsonProperty("awards")] public List<SignInAward>? Awards { get; set; }
    }

    public class SignInInfo {
        /// <summary>
        /// 累积签到天数
        /// </summary>
        [JsonProperty("total_sign_day")] public int TotalSignDay { get; set; }
        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        [JsonProperty("today")] public string? Today { get; set; }
        /// <summary>
        /// 今日是否已签到
        /// </summary>
        [JsonProperty("is_sign")] public bool IsSign { get; set; }
        public bool IsNotSign {
            get => !IsSign;
        }

        [JsonProperty("is_sub")] public bool IsSub { get; set; }
        [JsonProperty("first_bind")] public bool FirstBind { get; set; }
        [JsonProperty("month_first")] public bool MonthFirst { get; set; }
        [JsonProperty("sign_cnt_missed")] public bool SignCountMissed { get; set; }
    }

}
