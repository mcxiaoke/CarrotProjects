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
using System.Configuration;

namespace GenshinNotifier {
    public partial class MainForm : Form {

        private bool IsRefreshingData;

        public MainForm() {
            InitializeComponent();
        }

        private async void OnFormLoad(object sender, EventArgs e) {
            this.Text = $"{Application.ProductName} {Application.ProductVersion}";
            var cache =  DataController.Default.Cache;
            var user = await cache.LoadCache2<UserGameRole>();
            var note = await cache.LoadCache2<DailyNote>();
            Logger.Debug($"OnFormLoad uid={user?.GameUid} resin={note?.CurrentResin}");
            UpdateUIControls(user, note);
        }

        async void OnFormShow(object sender, EventArgs e) {
            var (user, error) = await DataController.Default.Initialize();
            Logger.Debug($"OnFormShow uid={user?.GameUid} error={error?.Message}");
            Logger.Debug(DataController.Default.Ready ? "Ready" : "NotReady");
            if (DataController.Default.Ready) {
                await RefreshDailyNote(sender, e);
            } else {
                AccountValueL.Text = "当前Cookie为空或已失效，请设置Cookie后使用";
                AccountValueL.ForeColor = Color.Red;
                if (error != null) {
                    Logger.Error("OnFormShow", error);
                }

            }
        }

        void ShowCookieDialog() {
            var cd = new CookieDialog {
                Location = new Point(this.Location.X + 160, this.Location.Y + 80)
            };
            cd.Handlers += OnCookieChanged;
            //cd.FormClosed += (fo, fe) => cd.Handlers -= OnCookieChanged;
            cd.ShowDialog();
        }

        async void OnCookieChanged(object sender, EventArgs e) {
            var evt = e as SimpleEventArgs;
            var newCookie = evt.Value;
            var oldCookie = DataController.Default.Cookie;
            if (newCookie != oldCookie) {
                Logger.Debug($"OnCookieChanged newCookie={newCookie}");
                DataController.Default.Cookie = newCookie;
                Properties.Settings.Default.MihoyoCookie = newCookie;
                Properties.Settings.Default.Save();
                await DataController.Default.Initialize();
                await RefreshDailyNote(null, null);
            } else {
                Logger.Debug($"OnCookieChanged not change");
            }

        }

        async void OnRefershButtonClicked(object sender, EventArgs e) {
            await RefreshDailyNote(sender, e);
        }

        void OnCookieButtonClicked(object sender, EventArgs e) {
            if (IsRefreshingData)
                return;
            ShowCookieDialog();
        }

        void OnOptionButtonClicked(object sender, EventArgs e) {
            if (IsRefreshingData)
                return;
        }

        void OnAboutButtonClicked(object sender, EventArgs e) {
            if (IsRefreshingData)
                return;
        }

        void UpdateRefreshState(bool loading) {
            IsRefreshingData = loading;
            this.RefreshButton.Enabled = !loading;
            //this.CookieButton.Enabled = !loading;
            this.LoadingPic.Visible = loading;
        }


        async Task RefreshDailyNote(object sender, EventArgs e) {
            UpdateRefreshState(true);
            var (user, note) = await DataController.Default.GetDailyNote();
            Logger.Info($"RefreshDailyNote\nuser={user?.GameUid}\resin={note?.CurrentResin}");
            UpdateUIControls(user, note);
            UpdateRefreshState(false);
        }

        void UpdateUIControls(UserGameRole user, DailyNote note) {
            if (user == null || note == null) {
                return;
            }
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

            UpdatedValueL.Text = note.CreatedAt.ToString("T");
        }

    }
}
