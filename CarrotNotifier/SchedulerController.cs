using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using CarrotCommon;
using GenshinLib;
using GenshinNotifier.Properties;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace GenshinNotifier {

    internal sealed class RemindConfig {
        public bool NotificationEnabled;
        public bool ResinEnabled;
        public bool HomeCoinEnabled;
        public bool DailyTaskEnabled;
        public bool DiscountEnabled;
        public bool ExpeditionEnabled;
        public bool TransformerEnabled;

        // resin max = 160, every 30 miniutes
        public const int ResinMax = 160;

        public const int ResinThreshold = ResinMax - 8;

        // home coin max = 2400, every 30 minutes
        public const int HomeCoinMax = 2400;

        public const int HomeCoinThreshold = HomeCoinMax - 50;

        // daily task, remind after 20:00 night
        public const int DailyTaskAfterHour = 20;

        // weekly discount, remind on sunday 12:00
        public const DayOfWeek DiscountAfterDay = DayOfWeek.Sunday;

        public const int DiscountAfterHour = 12;
        // expedition max = 5
        // transformer reached

        public bool Enabled => NotificationEnabled &&
            (ResinEnabled
            || HomeCoinEnabled
            || DailyTaskEnabled
            || DiscountEnabled
            || ExpeditionEnabled
            || TransformerEnabled);

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }

    internal sealed class RemindStatus {
        public DateTime StartAt;
        public DateTime LastCheckedAt;
        public DateTime LastNotifyAt;
        public DateTime LastSignAt;
        public DateTime LastVersionAt;

        public RemindStatus() {
            StartAt = DateTime.MinValue;
            LastCheckedAt = DateTime.MinValue;
            LastNotifyAt = DateTime.MinValue;
            LastSignAt = DateTime.MinValue;
            LastVersionAt = DateTime.MinValue;
        }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }

    public delegate void DataUpdateEventHandler(UserGameRole user, DailyNote note);

    public sealed class SchedulerController {

        //INTERVAL_NOTE every 30 minutes
        //INTERVAL_USER everty 4 hours;
        public const int TIME_ONE_SECOND_MS = 1000;

        public const int TIME_ONE_MINUTE_MS = 60 * TIME_ONE_SECOND_MS;
        public const int TIME_ONE_HOUR_MS = 60 * TIME_ONE_MINUTE_MS;
        public const int TIME_ONE_DAY_MS = 24 * TIME_ONE_HOUR_MS;
        public const int INTERVAL_CHECK = 5 * TIME_ONE_MINUTE_MS;
        public const int INTERVAL_NOTE = 20 * TIME_ONE_MINUTE_MS;
        public const int INTERVAL_USER = 4 * TIME_ONE_HOUR_MS;
        public const int INTERVAL_SIGN = 2 * TIME_ONE_HOUR_MS;

        public static SchedulerController Default {
            get { return lazy.Value; }
        }

        private static readonly Lazy<SchedulerController> lazy =
       new Lazy<SchedulerController>(() => new SchedulerController());

        private readonly RemindConfig Config;
        private readonly RemindStatus Status;
        private System.Timers.Timer ATimer;
        private string TodaySigned = null;
        private string TodaySignOKShown = null;
        private string TodaySignErrorShown = null;

        private static int ToastId = 0;

        private SchedulerController() {
            Config = new RemindConfig();
            Status = new RemindStatus();
        }

        public void Initialize() {
            Logger.Debug("SchedulerController.Initialize");
            LoadConfig();
            Setup();
            Start();
            CheckData("Initialize");
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e) {
            switch (e.Mode) {
                case PowerModes.Resume:
                    Logger.Debug($"===> Resume {DateTime.Now}");
                    break;

                case PowerModes.Suspend:
                    Logger.Debug($"===> Suspend {DateTime.Now}");
                    break;

                case PowerModes.StatusChange:
                    Logger.Debug($"===>  StatusChange {DateTime.Now}");
                    break;
            }
        }

        private void SystemEvents_SessionSwitch(object sender, EventArgs e) {
            var args = e as SessionSwitchEventArgs;
            Logger.Debug($"===> {args.Reason} {DateTime.Now}");
            if (args.Reason == SessionSwitchReason.SessionUnlock) {
                CheckData("SessionUnlock");
            }
        }

        private void LoadConfig() {
            Config.NotificationEnabled = Settings.Default.OptionEnableNotifications;
            Config.ResinEnabled = Settings.Default.OptionRemindResin;
            Config.HomeCoinEnabled = Settings.Default.OptionRemindCoin;
            Config.DailyTaskEnabled = Settings.Default.OptionRemindTask;
            Config.DiscountEnabled = Settings.Default.OptionRemindDiscount;
            Config.ExpeditionEnabled = Settings.Default.OptionRemindExpedition;
            Config.TransformerEnabled = Settings.Default.OptionRemindTransformer;
            //Logger.Debug($"SchedulerController.LoadConfig {Config.Enabled}");
        }

        private void Setup() {
            ATimer = new System.Timers.Timer {
                Interval = INTERVAL_CHECK
            };
            ATimer.Elapsed += CheckTimerEvent;
            ATimer.AutoReset = true;
            ATimer.Enabled = true;
        }

        private void Start() {
            Logger.Debug("SchedulerController.Start");
            ATimer.Start();
            Status.StartAt = DateTime.Now;
            Status.LastCheckedAt = DateTime.MinValue;
            Status.LastSignAt = DateTime.MinValue;
            Status.LastVersionAt = DateTime.MinValue;
        }

        public void Stop() {
            Logger.Debug("SchedulerController.Stop");
            if (ATimer != null) {
                ATimer.Stop();
                ATimer.Dispose();
            }
            Status.StartAt = DateTime.MinValue;
            Status.LastCheckedAt = DateTime.MinValue;
            Status.LastSignAt = DateTime.MinValue;
            Status.LastVersionAt = DateTime.MinValue;
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
            SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;
        }

        public void MuteToday() {
            var muteStart = DateTime.Now;
            var muteEnd = DateTime.Today.AddDays(1);
            var delta = muteEnd - muteStart;
            var muteOffset = DateTimeOffset.Now.AddHours(delta.Hours)
                .AddMinutes(delta.Minutes)
                .AddSeconds(delta.Seconds);
            // for debug, only mute 60 seconds
            //muteOffset = DateTimeOffset.Now.AddSeconds(60);
            Logger.Debug($"MuteToday util={muteOffset.ToLocalTime()}");
            CacheStore.Add(GetMuteKey(), "muted", muteOffset);
        }

        public void UnMuteToday() => CacheStore.Remove(GetMuteKey());

        private bool IsMutedToday => CacheStore.Get(GetMuteKey()) != null;

        private string GetMuteKey() {
            return "mute_" + DateTime.Now.ToShortDateString();
        }

        private void CheckTimerEvent(object sender, ElapsedEventArgs e) {
            LoadConfig();
            Logger.Debug($"CheckTimerEvent " +
                $"last={Status.LastCheckedAt} " +
                $"enabled={Config.Enabled} " +
                $"ready={DataController.Default.Ready} " +
                $"muted={IsMutedToday}");
            if (!Config.Enabled) { return; }
            if (!DataController.Default.Ready) { return; }
            Task.Run(async () => {
                await CheckUser();
                await CheckDailyNote("CheckTimerEvent");
                await CheckSignReward("CheckTimerEvent");
                await CheckVersionUpdate();
            }).Wait();
        }

        public void CheckData(string source) {
            Logger.Debug($"SchedulerController.CheckData {source}");
            Task.Run(async () => {
                if (!DataController.Default.Ready) {
                    await DataController.Default.Initialize();
                }
                if (!DataController.Default.Ready) {
                    return;
                }
                ShortcutHelper.EnableAutoStart(Settings.Default.OptionAutoStart);
                await AppUtils.CheckLocalAssets();
                await CheckUser(true);
                await CheckDailyNote("CheckOnLaunch", true);
                await Task.Delay(TIME_ONE_SECOND_MS * 5);
                await CheckSignReward("CheckData");
                await Task.Delay(TIME_ONE_SECOND_MS * 5);
                await CheckSharpUpdater();
                await CheckVersionUpdate();
            });
        }

        public void ForceRefresh() {
            if (!DataController.Default.Ready) { return; }
            Task.Run(async () => {
                await CheckUser(true);
                await CheckDailyNote("CheckDailyNote", true);
            });
        }

        private static FileVersionInfo ReadFileVersion(string path) {
            try {
                return FileVersionInfo.GetVersionInfo(path);
            } catch (Exception ex) {
                Logger.Error("ReadFileVersion", ex);
                return null;
            }
        }

        private async Task CheckVersionUpdate() {
            var checkElapsed = (DateTime.Now - Status.LastVersionAt);
            if (checkElapsed.TotalMilliseconds < TIME_ONE_DAY_MS - TIME_ONE_MINUTE_MS) { return; }
            Status.LastVersionAt = DateTime.Now;
            Logger.Debug($"CheckVersionUpdate");
            await AutoUpdater.CheckUpdate();
        }

        private async Task CheckSharpUpdater() {
            await Task.Run(() => {
                var exe = Path.Combine(System.AppContext.BaseDirectory, "SharpUpdater.exe");
                var exePending = exe + ".pending";
                if (!File.Exists(exePending)) {
                    return;
                }
                try {
                    var oldVer = ReadFileVersion(exe)?.ProductVersion;
                    var exeOld = exe + ".old";
                    if (File.Exists(exeOld)) {
                        File.Delete(exeOld);
                    }
                    if (File.Exists(exe)) {
                        File.Move(exe, exeOld);
                    }
                    if (File.Exists(exePending)) {
                        File.Move(exePending, exe);
                    }
                    File.Delete(exeOld);
                    var newVer = ReadFileVersion(exe)?.ProductVersion;
                    Logger.Info($"CheckSharpUpdater replaced old={oldVer} new={newVer}");
                } catch (Exception ex) {
                    Logger.Debug($"CheckSharpUpdater error={ex?.Message}");
                }
            });
        }

        private async Task CheckUser(bool forceUpdate = false) {
            var uc = DataController.Default.UserCached;
            var userLastUpdateAt = uc == null ? DateTime.MinValue : uc.CreatedAt;
            var userCheckElapsed = (DateTime.Now - userLastUpdateAt).TotalMilliseconds;
            if (userCheckElapsed > INTERVAL_USER + TIME_ONE_MINUTE_MS || forceUpdate) {
                var (user, ex) = await DataController.Default.GetGameRoleInfo("CheckUser");
                Logger.Info($"CheckUser refresh force={forceUpdate} uid={user?.GameUid} err={ex?.Message}");
            }
        }

        private async Task<DailyNote> CheckDailyNote(string source, bool forceUpdate = false) {
            var checkElapsed = (DateTime.Now - Status.LastCheckedAt);
            if (checkElapsed.TotalMilliseconds < INTERVAL_NOTE - TIME_ONE_MINUTE_MS) {
                if (!forceUpdate) {
                    return default;
                }
            }
            Status.LastCheckedAt = DateTime.Now;
            Logger.Debug($"CheckUser refresh force={forceUpdate} elapsed={checkElapsed.TotalMinutes} ({source})");
            try {
                var user = DataController.Default.UserCached;
                var (note, ex) = await DataController.Default.GetDailyNote();
                Logger.Info($"CheckDailyNote result uid={user?.GameUid} " +
                    $"resin={note?.CurrentResin} error={ex?.Message}");
                if (user != null && note != null) {
                    ShowNotification(user, note);
                }
            } catch (Exception ex) {
                Logger.Debug($"CheckDailyNote error={ex} ({source})");
            }
            return default;
        }

        public async Task ManualSignReward() {
            await DoSignReward(true);
        }

        // {"retcode":0,"message":"OK","data":{"total_sign_day":26,"today":"2022-05-27","is_sign":true,"first_bind":false,"is_sub":false,"
        public async Task CheckSignReward(string source) {
            if (!Settings.Default.OptionCheckinOnStart) {
                Logger.Debug($"CheckSignReward not enabled, skip");
                return;
            }
            var checkElapsed = (DateTime.Now - Status.LastSignAt);
            if (checkElapsed.TotalMilliseconds < INTERVAL_SIGN - TIME_ONE_MINUTE_MS) { return; }
            Status.LastSignAt = DateTime.Now;
            Logger.Debug($"CheckSignReward checkElapsed={checkElapsed.TotalMinutes} ({source})");
            var todayStr = DateTime.Now.ToString("yyyy-MM-dd");
            if (todayStr == TodaySigned) {
                Logger.Debug($"CheckSignReward today signed, skip {TodaySigned}");
                return;
            }
            await DoSignReward(false);
        }

        private async Task DoSignReward(bool manual) {
            Logger.Debug($"DoSignReward manual={manual}");
            Status.LastSignAt = DateTime.Now;
            var todayStr = DateTime.Now.ToString("yyyy-MM-dd");
            try {
                var (code, result, error) = await DataController.Default.PostSignReward();
                Logger.Info($"DoSignReward code={code} result={result} error={error?.Message}");
                dynamic obj = JsonConvert.DeserializeObject(result ?? error?.Message);
                if (code == 0 || code == -5003) {
                    var title = (code == 0) ?
                        $"本月已连续签到 {obj?.data?.total_sign_day ?? 0} 天" : "旅行者，你今天已经签到过了";
                    var text = $"今天是 {DateTime.Now.ToLongDateString()}\n记得到游戏里领取邮件奖励哦！";
                    TodaySigned = todayStr;
                    ShowSignOKNotification(title, text);
                } else {
                    ShowSignErrorNotification($"遇到错误 {obj.message}", result ?? error?.Message);
                }
            } catch (Exception ex) {
                Logger.Debug($"DoSignReward error={ex}");
                ShowSignErrorNotification($"遇到错误 {ex.GetType()}", ex.Message);
            }
        }

        private void ShowSignOKNotification(string title, string text) {
            if (!Settings.Default.OptionEnableNotifications) { return; }
            if (IsMutedToday) { return; }
            var todayNotiStr = DateTime.Now.ToString("yyyy-MM-dd");
            if (todayNotiStr == TodaySignOKShown) {
                Logger.Debug($"CheckSignReward today noti ok done, skip {TodaySigned}");
                return;
            }
            TodaySignOKShown = todayNotiStr;
            var image = AppUtils.IconFilePath;
            var user = DataController.Default.UserCached;
            var toast = new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Default)
                .AddHeader($"1002", $"{user.Nickname}，米游社原神签到成功", "action=signopen&id=1002")
                .AddText(title)
                .AddText(text)
                .AddAttributionText(DateTime.Now.ToString("F"))
                .AddAppLogoOverride(new Uri(image), ToastGenericAppLogoCrop.Circle);
            toast.Show(t => {
                t.Group = Application.ProductName;
                t.Tag = "SignReward";
                t.ExpirationTime = DateTimeOffset.Now.AddMinutes(5);
            });
        }

        private void ShowSignErrorNotification(string title, string text) {
            if (!Settings.Default.OptionEnableNotifications) { return; }
            if (IsMutedToday) { return; }
            var todayNotiStr = DateTime.Now.ToString("yyyy-MM-dd");
            if (todayNotiStr == TodaySignErrorShown) {
                Logger.Debug($"CheckSignReward today noti err done, skip {TodaySigned}");
                return;
            }
            TodaySignErrorShown = todayNotiStr;
            var image = AppUtils.IconFilePath;
            var user = DataController.Default.UserCached;
            var toast = new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Default)
                .AddHeader($"1003", $"{user.Nickname}，米游社原神签到失败", "action=signopen&id=1003")
                .AddText(title)
                .AddText(text)
                .AddAttributionText(DateTime.Now.ToString("F"))
                .AddAppLogoOverride(new Uri(image), ToastGenericAppLogoCrop.Circle);
            toast.Show(t => {
                t.Group = Application.ProductName;
                t.Tag = "SignReward";
                t.ExpirationTime = DateTimeOffset.Now.AddHours(5);
            });
        }

        public void ShowNotification(UserGameRole user, DailyNote note) {
            Logger.Debug($"ShowNotification " +
                $"uid={user?.GameUid} " +
                $"resin={note?.CurrentResin} " +
                $"enabled={Config.Enabled}");
            if (!Settings.Default.OptionEnableNotifications) { return; }
            if (IsMutedToday) { return; }
            var now = DateTime.Now;
            var title = new List<string>();
            var text = new List<string>();
            var needNotify = 0;
            if (Config.ResinEnabled) {
                if (note.ResinFull) {
                    title.Add("原粹树脂已溢出！");
                    ++needNotify;
                } else if (note.ResinAlmostFull()) {
                    title.Add("原粹树脂即将回满！");
                    ++needNotify;
                }
            }
            if (Config.HomeCoinEnabled) {
                if (note.HomeCoinFull) {
                    title.Add("洞天宝钱已溢出！");
                    ++needNotify;
                } else if (note.HomeCoinAlmostFull()) {
                    title.Add("洞天宝钱即将回满！");
                    ++needNotify;
                }
            }

            if (Config.DailyTaskEnabled) {
                if (!note.DailyTaskAllFinished) {
                    title.Add("每日委托未完成！");
                    if (now.Hour >= RemindConfig.DailyTaskAfterHour) {
                        ++needNotify;
                    }
                } else if (!note.IsExtraTaskRewardReceived) {
                    title.Add("每日委托奖励待领取！");
                    if (now.Hour >= RemindConfig.DailyTaskAfterHour) {
                        ++needNotify;
                    }
                }
            }

            if (Config.DiscountEnabled
                && now.DayOfWeek == RemindConfig.DiscountAfterDay) {
                if (!note.ResinDiscountAllUsed) {
                    title.Add("减半周本未完成！");
                    if (now.Hour > RemindConfig.DiscountAfterHour) {
                        ++needNotify;
                    }
                }
            }

            if (Config.ExpeditionEnabled) {
                if (note.ExpeditionAllFinished) {
                    title.Add("探索派遣已完成！");
                    ++needNotify;
                }
            }

            if (Config.TransformerEnabled) {
                if (note.TransformerReady) {
                    title.Add("参量质变仪已就绪！");
                    ++needNotify;
                }
            }

            if (needNotify == 0) {
                Logger.Debug($"ShowNotification no need, skip show");
                return;
            }

            text.Add($"原粹树脂 {note.CurrentResin}/{note.MaxResin}，" +
                $"洞天宝钱 {note.CurrentHomeCoin}/{note.MaxHomeCoin}");
            text.Add($"每日委托 {note.FinishedTaskNum}/{note.TotalTaskNum}，" +
                $"探索派遣 {note.CurrentExpeditionNum}/{note.MaxExpeditionNum}");
            text.Add($"减半周本 {note.ResinDiscountUsedNum}/{note.ResinDiscountNumLimit}，" +
                $"参量质变仪 {note.Transformer.RecoveryTime.TimeFormatted}");

            var titleStr = string.Concat(title.Take(3));
            var textStr = string.Join("，", text);
            var header = $"{user.Nickname}，你的{titleStr}";
            var image = AppUtils.IconFilePath;
            ++ToastId;
            var toast = new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Reminder)
                .AddArgument("toast_id", ToastId)
                .AddArgument("action", "click")
                .AddHeader($"{ToastId}", header, $"action=open&id={ToastId}")
                .AddText($"原神实时便签（{user.GameUid}）")
                .AddText(textStr)
                //.AddAttributionText(DateTime.Now.ToString("F"))
                .AddAppLogoOverride(new Uri(image), ToastGenericAppLogoCrop.Circle)
                // Buttons
                .AddButton(new ToastButton()
                    .SetContent("查看详情")
                    .AddArgument("action", "view")
                    .SetBackgroundActivation())
                .AddButton(new ToastButton()
                .SetContent("今日不再提醒")
                .AddArgument("action", "mute")
                .SetBackgroundActivation());
            toast.Show(t => {
                t.Group = Application.ProductName;
                t.Tag = "DailyNote";
                t.ExpirationTime = DateTimeOffset.Now.AddHours(1);
            });
            Status.LastNotifyAt = DateTime.Now;
            Logger.Info($"ShowNotification show hour={now.Hour} weekday={now.DayOfWeek} text={textStr}");
        }
    }
}