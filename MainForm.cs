using System;
using System.Text;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinNotifier.Net;
using GenshinNotifier.Properties;

namespace GenshinNotifier {
    public partial class MainForm : Form {

        private bool IsRefreshingData;

        public MainForm() {
            InitializeComponent();
        }

        private void PrintAllSettings() {
            Logger.Debug("---------- SETTINGS BEGIN ----------");
            var sb = new StringBuilder();
            foreach (SettingsProperty key in Settings.Default.Properties) {
                var value = Settings.Default[key.Name];
                sb.Append($"\n{key.Name} = {value}");
            }
            Logger.Debug(sb.ToString());
            Logger.Debug("---------- SETTINGS END ----------\n");
        }

        private async void OnFormLoad(object sender, EventArgs e) {
            PrintAllSettings();
            this.Text = $"{Application.ProductName} {Application.ProductVersion}";
            var cache = DataController.Default.Cache;
            var user = await cache.LoadCache2<UserGameRole>();
            var note = await cache.LoadCache2<DailyNote>();
            Logger.Debug($"OnFormLoad cached uid={user?.GameUid} resin={note?.CurrentResin}");
            UpdateUIControls(user, note);
        }

        async void OnFormShow(object sender, EventArgs e) {
            var (user, error) = await DataController.Default.Initialize();
            Logger.Debug($"OnFormShow uid={user?.GameUid} error={error?.Message}");
            Logger.Debug(DataController.Default.Ready ? "Ready" : "NotReady");
            if (DataController.Default.Ready) {
                if (Settings.Default.OptionRefreshOnStart) {
                    await RefreshDailyNote(sender, e);
                }
            } else {
                AccountValueL.Text = "当前Cookie为空或已失效，请设置Cookie后使用";
                AccountValueL.ForeColor = Color.Red;
            }
        }

        void ShowCookieDialog() {
            var cd = new CookieDialog {
                Location = new Point(this.Location.X + 120, this.Location.Y + 80)
            };
            cd.Handlers += OnCookieChanged;
            cd.FormClosed += (fo, fe) => cd.Handlers -= OnCookieChanged;
            cd.ShowDialog();
            Logger.Debug("ShowCookieDialog");
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
                Logger.Debug($"OnCookieChanged data saved");
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
            var cd = new OptionForm {
                Location = new Point(this.Location.X + 120, this.Location.Y + 80)
            };
            cd.ShowDialog();
        }

        void UpdateRefreshState(bool loading) {
            IsRefreshingData = loading;
            this.RefreshButton.Enabled = !loading;
            this.CookieButton.Enabled = !loading;
            this.LoadingPic.Visible = loading;
        }


        async Task RefreshDailyNote(object sender, EventArgs e) {
            Logger.Debug("RefreshDailyNote");
            UpdateRefreshState(true);
            var (user, note) = await DataController.Default.GetDailyNote();
            Logger.Info($"RefreshDailyNote user={user?.GameUid} resin={note?.CurrentResin}");
            UpdateUIControls(user, note);
            UpdateRefreshState(false);
        }

        void UpdateUIControls(UserGameRole user, DailyNote note) {
            if (user == null || note == null) {
                return;
            }
            Logger.Debug("UpdateUIControls");
            var colorNormal = Color.Green;
            var colorAttention = Color.Red;
            AccountValueL.Text = $"{user.Nickname} {user.Level}级 / {user.RegionName}({user.Server}) / {user.GameUid}";
            AccountValueL.ForeColor = Color.Blue;
            var resinMayFull = note.CurrentResin >= note.MaxResin - 2;
            ResinValueL.Text = $"{note.CurrentResin}/{note.MaxResin}";
            ResinValueL.ForeColor = resinMayFull ? colorAttention : colorNormal;
            ResinRecValueL.Text = $"{note.ResinRecoveryTimeFormatted}";
            ResinRecValueL.ForeColor = resinMayFull ? colorAttention : colorNormal;
            ResinTimeValueL.Text = $"{note.ResinRecoveryTargetTimeFormatted}";
            ResinTimeValueL.ForeColor = resinMayFull ? colorAttention : colorNormal;

            var expeditionCompleted = note.Expeditions?.All(it => it.RemainedTime == "0") ?? false;
            var expeditionText = $"{note.CurrentExpeditionNum}/{note.MaxExpeditionNum}";
            if (expeditionCompleted) {
                expeditionText += " (已完成)";
            }
            ExpeditionValueL.Text = expeditionText;
            ExpeditionValueL.ForeColor = expeditionCompleted ? colorAttention : colorNormal;

            var taskRewardStr = note.IsExtraTaskRewardReceived ? "(已领取)" : "(未领取)";
            TaskNameValueL.Text = $"{note.FinishedTaskNum}/{note.TotalTaskNum} {taskRewardStr}";
            TaskNameValueL.ForeColor = note.IsExtraTaskRewardReceived ? colorNormal : colorAttention;

            var homeCoinMayFull = note.CurrentHomeCoin >= note.MaxHomeCoin - 100;
            HomeCoinValueL.Text = $"{note.CurrentHomeCoin}/{note.MaxHomeCoin}";
            HomeCoinValueL.ForeColor = homeCoinMayFull ? colorAttention : colorNormal;

            var discountNotUsed = note.ResinDiscountUsedNum < note.ResinDiscountNumLimit;
            DiscountTaskValueL.Text = $"{note.ResinDiscountUsedNum}/{note.ResinDiscountNumLimit}";
            DiscountTaskValueL.ForeColor = discountNotUsed ? colorAttention : colorNormal;

            var transformerReady = note.Transformer.RecoveryTime.Reached;
            TransformerValueL.Text = $"{note.Transformer.RecoveryTime.TimeFormatted}";
            TransformerValueL.ForeColor = (transformerReady ? colorAttention : colorNormal);

            UpdatedValueL.Text = note.CreatedAt.ToString("T");
            UpdatedValueL.ForeColor = colorNormal;
        }

        private void HideToTrayIcon() {
            this.Hide();
            this.ShowInTaskbar = false;
            AppNotifyIcon.Visible = true;
            //AppNotifyIcon.ShowBalloonTip(2000, "已最小化到系统托盘", "双击图标恢复", ToolTipIcon.Info);
        }

        private void RestoreFromTrayIcon() {
            this.Show();
            this.Activate();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            AppNotifyIcon.Visible = false;
        }

        private void OnSizeChanged(object sender, EventArgs e) {
            Logger.Info($"OnSizeChanged {this.WindowState}");
            if (this.WindowState == FormWindowState.Minimized) {
                HideToTrayIcon();
            }
        }

        private void AppNotifyIcon_DoubleClick(object sender, EventArgs e) {
            RestoreFromTrayIcon();
        }

        private void MenuItemShow_Click(object sender, EventArgs e) {
            RestoreFromTrayIcon();
        }

        private void MenuItemCheckin_Click(object sender, EventArgs e) {

        }

        private void MenuItemQuit_Click(object sender, EventArgs e) {
            Dispose();
            Close();
        }
    }
}
