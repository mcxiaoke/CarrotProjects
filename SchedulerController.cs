using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using GenshinNotifier.Properties;
using Newtonsoft.Json;
using GenshinNotifier.Net;

namespace GenshinNotifier {

    sealed class RemindConfig {
        public bool Resin;
        public bool HomeCoin;
        public bool DailyTask;
        public bool Discount;
        public bool Expedition;
        public bool Transformer;

        public bool Enabled => Resin || HomeCoin || DailyTask || Discount || Expedition || Transformer;

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
        public (UserGameRole, DailyNote) PendingData { get; set; }

        private static readonly int INTERVAL_NOTE = 10 * 60 * 1000; // in milliseconds
        private static readonly int INTERVAL_USER = 4 * 60 * 60 * 1000;
        private readonly RemindConfig Config;
        private readonly RemindStatus Status;
        private System.Timers.Timer ATimer;

        private SchedulerController() {
            Config = new RemindConfig();
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
        }

        private void Setup() {
            ATimer = new Timer();
            ATimer.Interval = 30 * 1000;
            ATimer.Elapsed += TimerEvent;
            ATimer.AutoReset = true;
            ATimer.Enabled = true;
        }

        private void Start() {
            ATimer.Start();
            Status.StartAt = DateTime.Now;
            Status.LastCheckedAt = DateTime.MinValue;
        }

        public void Stop() {
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
                var (user, note) = await DataController.Default.GetDailyNote();
                Logger.Info($"TimerEvent uid={user?.GameUid} resin={note?.CurrentResin}");
                if (user != null && note != null) {
                    Status.user = user;
                    Status.note = note;
                    Handlers?.Invoke(user, note);
                    PendingData = (user, note);
                }
            }).Wait();
        }
    }
}
