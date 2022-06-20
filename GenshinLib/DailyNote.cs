using System;
using System.Collections.Generic;
using System.Text;
using Carrot.Common;
using Carrot.Common.Extensions;
using Newtonsoft.Json;

namespace GenshinLib {

    /// <summary>
    /// 参量质变仪恢复时间包装
    /// 已准备完成 $后可再次使用
    /// 冷却中     可使用
    /// </summary>
    public class RecoveryTime {

        /// <summary>
        /// 日
        /// </summary>
        [JsonProperty("Day")]
        public int Day { get; set; }

        /// <summary>
        /// 时
        /// </summary>
        [JsonProperty("Hour")]
        public int Hour { get; set; }

        /// <summary>
        /// 分
        /// </summary>
        [JsonProperty("Minute")]
        public int Minute { get; set; }

        /// <summary>
        /// 秒
        /// </summary>
        [JsonProperty("Second")]
        public int Second { get; set; }

        /// <summary>
        /// 是否已经到达
        /// </summary>
        [JsonProperty("reached")]
        public bool Reached { get; set; }

        /// <summary>
        /// 获取格式化的剩余时间
        /// </summary>
        public string TimeFormatted {
            get {
                if (Reached) {
                    return "可使用";
                } else {
                    return new StringBuilder()
                        .AppendIf(Day > 0, $"{Day}天")
                        .AppendIf(Hour > 0, $"{Hour}小时")
                        .AppendIf(Minute > 0, $"{Minute}分")
                        .Append("后")
                        .ToString();
                }
            }
        }

        /// <summary>
        /// 获取格式化的状态
        /// </summary>
        public string ReachedFormatted {
            get => Reached ? "可使用" : "冷却中";
        }

        public override string ToString() {
            return Utility.Stringify(this);
        }
    }

    /// <summary>
    /// 参量质变仪
    /// </summary>
    public class Transformer {

        /// <summary>
        /// 是否拥有该道具
        /// </summary>
        [JsonProperty("obtained")]
        public bool Obtained { get; set; }

        /// <summary>
        /// 恢复时间包装
        /// </summary>
        [JsonProperty("recovery_time")]
        public RecoveryTime? RecoveryTime { get; set; }

        /// <summary>
        /// Wiki链接
        /// </summary>
        //[JsonProperty("wiki")]
        //public string Wiki { get; set; }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// 探索派遣
    /// </summary>
    public class Expedition {
        /// <summary>
        /// 图标
        /// </summary>
        //[JsonProperty("avatar_side_icon")]
        //public string AvatarSideIcon { get; set; }

        /// <summary>
        /// 状态 Ongoing:派遣中 Finished:已完成
        /// </summary>
        [JsonProperty("status")]
        public string? Status { get; set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        [JsonProperty("remained_time")]
        public string? RemainedTime { get; set; }

        /// <summary>
        /// 格式化的剩余时间
        /// </summary>
        public string RemainedTimeFormatted {
            get {
                if (Status == "Finished") {
                    return "已完成";
                }

                if (RemainedTime != null) {
                    var ts = new TimeSpan(0, 0, int.Parse(RemainedTime));
                    return ts.Hours > 0 ? $"{ts.Hours}时" : $"{ts.Minutes}分";
                }

                return string.Empty;
            }
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class DailyNote : ICloneable {
        public static string GetDayName(int days) {
            return days switch {
                0 => "今天",
                1 => "明天",
                2 => "后天",
                _ => $"{days}天",
            };
        }

        // added for convenince
        public bool ResinDiscountAllUsed => ResinDiscountUsedNum >= ResinDiscountNumLimit;

        public bool ResinAlmostFull(int offset = 8) => CurrentResin >= MaxResin - offset;

        public bool ResinFull => CurrentResin >= MaxResin;

        public bool HomeCoinAlmostFull(int offset = 100) => CurrentHomeCoin >= MaxHomeCoin - offset;

        public bool HomeCoinFull => CurrentHomeCoin >= MaxHomeCoin;
        public bool DailyTaskAllFinished => FinishedTaskNum >= TotalTaskNum;
        public bool ExpeditionAllFinished => Expeditions?.FindAll(it => it.RemainedTime == "0").Count >= CurrentExpeditionNum;
        public bool TransformerReady => Transformer?.RecoveryTime?.Reached ?? false;

        /// <summary>
        /// 数据更新时间
        /// </summary>
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 当前树脂
        /// </summary>
        [JsonProperty("current_resin")]
        public int CurrentResin { get; set; }

        /// <summary>
        /// 最大树脂
        /// </summary>
        [JsonProperty("max_resin")]
        public int MaxResin { get; set; }

        /// <summary>
        /// 树脂恢复时间 <see cref="string"/>类型的秒数
        /// </summary>
        [JsonProperty("resin_recovery_time")]
        public string ResinRecoveryTime { get; set; } = string.Empty;

        /// <summary>
        /// 获取格式化的剩余时间
        /// </summary>
        public string ResinRecoveryTimeFormatted {
            get {
                if (string.IsNullOrEmpty(ResinRecoveryTime)) {
                    return "已回满";
                }
                if (int.Parse(ResinRecoveryTime) < 60) {
                    return "已回满";
                }
                var Hour = int.Parse(ResinRecoveryTime) / 3600;
                var Minute = int.Parse(ResinRecoveryTime) % 3600 / 60;
                return new StringBuilder()
                    .AppendIf(Hour > 0, $"{Hour}小时")
                    .AppendIf(Minute > 0, $"{Minute}分")
                    .ToString();
            }
        }

        /// <summary>
        /// 格式化的树脂恢复时间
        /// </summary>
        public string ResinRecoveryTargetTimeFormatted {
            get {
                if (string.IsNullOrEmpty(ResinRecoveryTime)) {
                    return "已回满";
                }
                if (int.Parse(ResinRecoveryTime) < 60) {
                    return "已回满";
                }
                if (ResinRecoveryTime != null) {
                    DateTime tt = DateTime.Now.AddSeconds(int.Parse(ResinRecoveryTime));
                    int totalDays = (tt - DateTime.Today).Days;
                    string day = GetDayName(totalDays);
                    return $"{day} {tt:HH:mm}";
                }
                return "";
            }
        }

        /// <summary>
        /// 委托完成数
        /// </summary>
        [JsonProperty("finished_task_num")]
        public int FinishedTaskNum { get; set; }

        /// <summary>
        /// 委托总数
        /// </summary>
        [JsonProperty("total_task_num")]
        public int TotalTaskNum { get; set; }

        /// <summary>
        /// 4次委托额外奖励是否领取
        /// </summary>
        [JsonProperty("is_extra_task_reward_received")]
        public bool IsExtraTaskRewardReceived { get; set; }

        /// <summary>
        /// 每日委托奖励字符串
        /// </summary>
        public string ExtraTaskRewardDescription {
            get {
                return IsExtraTaskRewardReceived
                    ? "已领取「每日委托」奖励"
                    : FinishedTaskNum == TotalTaskNum
                        ? "「每日委托」奖励待领取"
                        : "今日完成委托次数不足";
            }
        }

        /// <summary>
        /// 剩余周本折扣次数
        /// </summary>
        [JsonProperty("remain_resin_discount_num")]
        public int RemainResinDiscountNum { get; set; }

        /// <summary>
        /// 周本树脂减免使用次数
        /// </summary>
        public int ResinDiscountUsedNum {
            get => ResinDiscountNumLimit - RemainResinDiscountNum;
        }

        /// <summary>
        /// 周本折扣总次数
        /// </summary>
        [JsonProperty("resin_discount_num_limit")]
        public int ResinDiscountNumLimit { get; set; }

        /// <summary>
        /// 当前派遣数
        /// </summary>
        [JsonProperty("current_expedition_num")]
        public int CurrentExpeditionNum { get; set; }

        /// <summary>
        /// 最大派遣数
        /// </summary>
        [JsonProperty("max_expedition_num")]
        public int MaxExpeditionNum { get; set; }

        /// <summary>
        /// 派遣
        /// </summary>
        [JsonProperty("expeditions")]
        public List<Expedition>? Expeditions { get; set; }

        /// <summary>
        /// 当前洞天宝钱
        /// </summary>
        [JsonProperty("current_home_coin")]
        public int CurrentHomeCoin { get; set; }

        /// <summary>
        /// 最大洞天宝钱
        /// </summary>
        [JsonProperty("max_home_coin")]
        public int MaxHomeCoin { get; set; }

        /// <summary>
        /// 洞天宝钱恢复时间 <see cref="string"/>类型的秒数
        /// </summary>
        [JsonProperty("home_coin_recovery_time")]
        public string HomeCoinRecoveryTime { get; set; } = string.Empty;

        /// <summary>
        /// 格式化的洞天宝钱恢复时间
        /// </summary>
        public string HomeCoinRecoveryTargetTimeFormatted {
            get {
                if (HomeCoinRecoveryTime != null) {
                    DateTime tt = DateTime.Now.AddSeconds(int.Parse(HomeCoinRecoveryTime));
                    int totalDays = (tt - DateTime.Today).Days;
                    string day = GetDayName(totalDays);
                    return $"{day} {tt:HH:mm}";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 参量质变仪
        /// </summary>
        [JsonProperty("transformer")]
        public Transformer? Transformer { get; set; }

        public override string ToString() {
            return Utility.Stringify(this);
        }

        public object Clone() {
            return this.MemberwiseClone();
        }
    }
}