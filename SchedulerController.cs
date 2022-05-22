using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GenshinNotifier.Properties;
using Newtonsoft.Json;
using GenshinNotifier.Net;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GenshinNotifier {

    sealed class RemindConfig {
        public bool Resin;
        public bool HomeCoin;
        public bool DailyTask;
        public bool Discount;
        public bool Expedition;
        public bool Transformer;

        public bool Enabled => Resin
            || HomeCoin
            || DailyTask
            || Discount
            || Expedition
            || Transformer;

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }

    sealed class RemindStatus {
        public DateTime StartAt;
        public DateTime LastCheckedAt;
        public UserGameRole user;
        public DailyNote note;

        public RemindStatus() {
            StartAt = DateTime.MinValue;
            LastCheckedAt = DateTime.MinValue;
            user = null;
            note = null;
        }

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }

    public delegate void DataUpdateEventHandler(UserGameRole user, DailyNote note);

    public sealed class SchedulerController {

        public static int ThreadId => System.Threading.Thread.CurrentThread.ManagedThreadId;

        public static SchedulerController Default = new SchedulerController();
        public DataUpdateEventHandler Handlers;

        private static readonly int INTERVAL_NOTE = 10 * 60 * 1000; // in milliseconds
        private static readonly int INTERVAL_USER = 4 * 60 * 60 * 1000;
        private readonly RemindConfig Config;
        private readonly RemindConfig MuteConfig;
        private readonly RemindStatus Status;
        private Timer ATimer;

        private SchedulerController() {
            Config = new RemindConfig();
            MuteConfig = new RemindConfig();
            Status = new RemindStatus();
        }

        public void Initialize() {
            Logger.Debug("SchedulerController.Initialize");
            LoadConfig();
            Setup();
            Start();
        }

        private void LoadConfig() {
            Config.Resin = Settings.Default.OptionRemindResin;
            Config.HomeCoin = Settings.Default.OptionRemindCoin;
            Config.DailyTask = Settings.Default.OptionRemindTask;
            Config.Discount = Settings.Default.OptionRemindDiscount;
            Config.Expedition = Settings.Default.OptionRemindExpedition;
            Config.Transformer = Settings.Default.OptionRemindTransformer;
            Logger.Debug($"SchedulerController.LoadConfig {Config}");
        }

        private void Setup() {
            ATimer = new Timer();
            ATimer.Interval = 60 * 1000;
            ATimer.Elapsed += TimerEvent;
            ATimer.AutoReset = true;
            ATimer.Enabled = true;
        }

        private void Start() {
            Logger.Debug("SchedulerController.Start");
            ATimer.Start();
            Status.StartAt = DateTime.Now;
            Status.LastCheckedAt = DateTime.MinValue;
        }

        public void Stop() {
            Logger.Debug("SchedulerController.Stop");
            Status.StartAt = DateTime.MinValue;
            Status.LastCheckedAt = DateTime.MinValue;
            ATimer.Stop();
            ATimer.Dispose();
        }

        private void TimerEvent(object sender, ElapsedEventArgs e) {
            Logger.Debug($"TimerEvent last={Status.LastCheckedAt} now={e.SignalTime}");
            Status.LastCheckedAt = DateTime.Now;
            if (!DataController.Default.Ready) {
                Logger.Debug($"TimerEvent not ready, skip task");
                return;
            }
            Task.Run(async () => {
                //todo fixme for debug
                Logger.Debug($"====== TimerEvent ====== {ThreadId}");
                var n = DataController.Default.NoteCached;
                if (n != null) { return; }
                var (user, note) = await DataController.Default.GetDailyNote();
                Logger.Info($"TimerEvent uid={user?.GameUid} resin={note?.CurrentResin}");
                if (user != null && note != null) {
                    LoadConfig();
                    Status.user = user;
                    Status.note = note;
                    Handlers?.Invoke(user, note);
                }
            }).Wait();
        }
    }
}
