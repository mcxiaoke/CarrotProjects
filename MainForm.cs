using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinNotifier.Net;

namespace GenshinNotifier {
    public partial class MainForm : Form {

        private readonly DataController _controller;
        private readonly string _defaultTitle;

        public MainForm() {
            InitializeComponent();
            _controller = new DataController();
            _defaultTitle = this.Text;
            this.Shown += OnFormShow;
            this.RefershButton.Click += OnRefershButtonClicked;
            var am = AssemblyName.GetAssemblyName("GenshinNotifier.exe");
            Logger.Info(am.Name);
            this.Text = $"{_defaultTitle} v{am.Version}";
        }

        async void OnFormShow(object sender, EventArgs e) {
            await RefreshDailyNote(sender, e);
        }

        async void OnRefershButtonClicked(object sender, EventArgs e) {
            await RefreshDailyNote(sender, e);
        }

        void UpdateRefreshState(bool loading) {
            this.RefershButton.Enabled = !loading;
            this.RefershButton.Text = loading ? "更新中..." : "立即更新";
            this.CookieButton.Enabled = !loading;
            this.LoadingPic.Visible = loading;
        }

        async Task RefreshDailyNote(object sender, EventArgs e) {
            Logger.Debug("RefreshDailyNote begin");
            UpdateRefreshState(true);
            var (user, note) = await _controller.FetchData();
            this.RefershButton.Enabled = true;
            Logger.Info($"RefreshDailyNote\nuser={user.GameUid}\nnote={note.CurrentResin}");
            if (user == null || note == null) {
                UpdateRefreshState(false);
                return;
            }

            Logger.Debug("RefreshDailyNote data");
            AccountValueL.Text = $"{user.Nickname} {user.Level}级 / {user.RegionName}({user.Server}) / {user.GameUid}";
            AccountValueL.ForeColor = Color.Blue;


            var resinMayFull = note.CurrentResin >= note.MaxResin - 2;
            ResinValueL.Text = $"{note.CurrentResin}/{note.MaxResin}";
            ResinValueL.ForeColor = resinMayFull ? Color.Red : Color.Green;
            ResinRecValueL.Text = $"{note.ResinRecoveryTimeFormatted}";
            ResinRecValueL.ForeColor = resinMayFull ? Color.Red : Control.DefaultForeColor;
            ResinTimeValueL.Text = $"{note.ResinRecoveryTargetTimeFormatted}";
            ResinTimeValueL.ForeColor = resinMayFull ? Color.Red : Control.DefaultForeColor;

            var expeditionCompleted = note.Expeditions?.All(it => it.RemainedTime == "0") ?? false;
            var expeditionText = $"{note.CurrentExpeditionNum}/{note.MaxExpeditionNum}";
            if (expeditionCompleted) {
                expeditionText += " (已完成)";
            }
            ExpeditionValueL.Text = expeditionText;
            ExpeditionValueL.ForeColor = expeditionCompleted ? Color.Red : Control.DefaultForeColor;

            var taskRewardStr = note.IsExtraTaskRewardReceived ? "(已领取)" : "(未领取)";
            TaskNameValueL.Text = $"{note.FinishedTaskNum}/{note.TotalTaskNum} {taskRewardStr}";
            TaskNameValueL.ForeColor = note.IsExtraTaskRewardReceived ? Control.DefaultForeColor : Color.Red;

            var homeCoinMayFull = note.CurrentHomeCoin >= note.MaxHomeCoin - 100;
            HomeCoinValueL.Text = $"{note.CurrentHomeCoin}/{note.MaxHomeCoin}";
            HomeCoinValueL.ForeColor = homeCoinMayFull ? Color.Red : Control.DefaultForeColor;

            var discountNotUsed = note.ResinDiscountUsedNum < note.ResinDiscountNumLimit;
            DiscountTaskValueL.Text = $"{note.ResinDiscountUsedNum}/{note.ResinDiscountNumLimit}";
            DiscountTaskValueL.ForeColor = discountNotUsed ? Color.Red : Control.DefaultForeColor;

            var transformerReady = note.Transformer.RecoveryTime.Reached;
            TransformerValueL.Text = $"{note.Transformer.RecoveryTime.TimeFormatted}";
            TransformerValueL.ForeColor = (transformerReady ? Color.Red : Control.DefaultForeColor);

            UpdatedValueL.Text = DateTime.Now.ToString("T");
            Logger.Debug("RefreshDailyNote end");
            UpdateRefreshState(false);
        }
    }
}
