using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CarrotCommon;
using GenshinLib;
using GenshinNotifier.Properties;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GenshinNotifier {

    public partial class MainForm : Form {
        private long _refreshing = 0;

        public bool IsRefreshingData {
            get {
                return Interlocked.Read(ref _refreshing) == 1;
            }
            set {
                Interlocked.Exchange(ref _refreshing, Convert.ToInt64(value));
            }
        }

        private DateTime LastUpdateTime = DateTime.MinValue;
        private readonly bool HideOnStart = false;

        public MainForm(bool shouldHide) {
            HideOnStart = shouldHide;
            //TopMost = true;
            Logger.Debug($"MainForm HideOnStart={HideOnStart}");
            InitializeComponent();
            Logger.Debug(AppInfo.AsString());
            Logger.Debug(AppInfo.CommonAppDataPath);
            Logger.Debug(AppInfo.LocalAppDataPath);
            Logger.Debug(AppInfo.RoamingAppDataPath);
        }

        private void PrintAllSettings() {
            Logger.Debug("---------- SETTINGS BEGIN ----------");
            var sb = new StringBuilder();
            foreach (SettingsProperty key in Settings.Default.Properties) {
                var value = Settings.Default[key.Name];
                sb.Append('\n').Append(key.Name).Append(" = ").Append(value);
            }
            Logger.Debug(sb.ToString());
            Logger.Debug("---------- SETTINGS END ----------\n");
        }

        private bool IsFormLoaded = false;

        private async void OnFormLoad(object sender, EventArgs e) {
            Logger.Debug("OnFormLoad()");
            IsFormLoaded = true;
            if (HideOnStart) {
                HideToTrayIcon();
            }
            //PrintAllSettings();
            this.Text = $"{Application.ProductName} {Application.ProductVersion}";
            var (user, error) = await DataController.Default.Initialize();
            Logger.Debug($"OnFormLoad uid={user?.GameUid} error={error?.Message}");
            Logger.Debug(DataController.Default.Ready ? "Ready" : "NotReady");
            if (DataController.Default.Ready) {
                var uc = DataController.Default.UserCached;
                var nc = DataController.Default.NoteCached;
                UpdateUIControls(uc, nc);
                if (Settings.Default.OptionRefreshOnStart
                    || string.IsNullOrEmpty(UpdatedValueL.Text)) {
                    await RefreshDailyNote(sender, e);
                }
            } else {
                AccountValueL.Text = "无帐号或Cookie失效，请点击帐号按钮设置";
                AccountValueL.ForeColor = Color.Blue;
            }
            ToastNotificationManagerCompat.OnActivated += OnNotificationActivated;
            Settings.Default.PropertyChanged += OnSettingValueChanged;
            if (Settings.Default.FirstLaunch) {
                Settings.Default.FirstLaunch = false;
                Settings.Default.Save();
                OnFirstLaunch();
            }
        }

        private void OnFormShow(object sender, EventArgs e) {
            Logger.Debug($"OnFormShow() hide={HideOnStart}");
            if (DataController.Default.Ready) {
                StopCookieBlinkTimer();
            } else {
                StartCookieBlinkTimer();
            }
        }

        private void OnFirstLaunch() {
            Logger.Info("OnFirstLaunch");
        }

        public void ShowMyWindow(object sender, EventArgs e) {
            Logger.Debug($"ShowWindowAsync() {sender}");
            Invoke(new Action(() => RestoreFromTrayIcon()));
        }

        private System.Timers.Timer CookieBlinkTimer;

        private void StartCookieBlinkTimer() {
            //Logger.Debug("StartCookieBlinkTimer");
            CookieBlinkTimer = new System.Timers.Timer {
                Interval = 250
            };
            CookieBlinkTimer.Elapsed += CookieBlinkEvent;
            CookieBlinkTimer.Start();
        }

        private void StopCookieBlinkTimer() {
            if (CookieBlinkTimer != null) {
                //Logger.Debug("StopCookieBlinkTimer");
                CookieBlinkTimer.Stop();
                CookieBlinkTimer = null;
                CookieButton.ForeColor = default;
                CookieButton.BackColor = SystemColors.ButtonFace;
                CookieButton.UseVisualStyleBackColor = true;
            }
        }

        private void CookieBlinkEvent(object sender, EventArgs e) {
            if (CookieButton.ForeColor != Color.White) {
                CookieButton.ForeColor = Color.White;
            } else {
                CookieButton.ForeColor = default;
            }
            if (CookieButton.BackColor != Color.Blue) {
                CookieButton.BackColor = Color.Blue;
            } else {
                CookieButton.BackColor = SystemColors.ButtonFace;
            }
        }

        private void OnNotificationActivated(ToastNotificationActivatedEventArgsCompat toastArgs) {
            Logger.Debug($"OnNotificationActivated {toastArgs.Argument}");
            // Obtain the arguments from the notification
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);
            var action = args.Get("action");
            // Need to dispatch to UI thread if performing UI operations
            if (action == "view") {
                // restore from tray
                Invoke(new Action(() => {
                    RestoreFromTrayIcon();
                    UpdateUIControlsUseCache();
                }));
            } else if (action == "mute") {
                // mute current day
                SchedulerController.Default.MuteToday();
            } else if (action == "update") {
                AutoUpdater.ShowUpdater();
            }
        }

        private void OnFormActivated(object sender, EventArgs e) {
            Logger.Debug($"OnFormActivated() visible={this.Visible} state={this.WindowState}");
        }

        private async void OnVisibleChanged(object sender, EventArgs e) {
            Logger.Debug($"OnVisibleChanged() visible={this.Visible} formLoaded={IsFormLoaded}");
            if (!this.Visible) { return; }
            if (!IsFormLoaded) { return; }
            if (!DataController.Default.Ready) { return; }
            if (IsRefreshingData) { return; }
            UpdateUIControlsUseCache();
            var note = DataController.Default.NoteCached;
            var needRefresh = note != null && (DateTime.Now - note.CreatedAt).TotalMilliseconds > SchedulerController.TIME_ONE_MINUTE_MS * 5;
            if (needRefresh) {
                await RefreshDailyNote(null, null);
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            Logger.Debug($"OnFormClosing() {Settings.Default.OptionCloseConfirm}");
            StopCookieBlinkTimer();
            if (!DataController.Default.Ready) {
                // no account set, quit now
                return;
            }
            if (e.CloseReason == CloseReason.UserClosing) {
                if (Settings.Default.OptionCloseConfirm) {
                    var cd = new ConfirmDialog("退出确认", "最小化到系统托盘", "直接退出") {
                        Location = new Point(this.Location.X + 80, this.Location.Y + 120),
                        Owner = this
                    };
                    var ret = cd.ShowDialog();
                    //Logger.Debug($"ConfirmDialog {e.CloseReason} {ret}");
                    switch (ret) {
                        case DialogResult.Cancel:
                            // close button
                            e.Cancel = true;
                            break;

                        case DialogResult.No:
                            // hide button
                            e.Cancel = true;
                            HideToTrayIcon();
                            break;

                        default:
                            break;
                    }
                } else {
                    e.Cancel = true;
                    HideToTrayIcon();
                }
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e) {
            Logger.Debug("OnFormClosed");
            IsFormLoaded = false;
            Settings.Default.PropertyChanged -= OnSettingValueChanged;
            Settings.Default.Save();
        }

        private void OnSettingValueChanged(object sender, PropertyChangedEventArgs e) {
            var key = e.PropertyName;
            var value = Settings.Default[key];
            Logger.Debug($"OnSettingValueChanged: {key}={value}");
            if (key == "OptionAutoStart") {
                Task.Run(() => ShortcutHelper.EnableAutoStart(Settings.Default.OptionAutoStart));
            }
        }

        private void ShowCookieDialog() {
            var cd = new CookieDialog {
                Location = new Point(this.Location.X + 80, this.Location.Y + 120),
                Owner = this
            };
            cd.CookieHandlers += OnCookieChanged;
            cd.FormClosed += (fo, fe) => cd.CookieHandlers -= OnCookieChanged;
            cd.ShowDialog();
            Logger.Debug("ShowCookieDialog");
        }

        private async void OnCookieChanged(object sender, EventArgs e) {
            var evt = e as SimpleEventArgs;
            var newCookie = evt.Value;
            if (string.IsNullOrWhiteSpace(newCookie)) {
                // clear cookie event
                DataController.Default.ClearUserData();
                Application.Restart();
                return;
            }
            var oldCookie = DataController.Default.Cookie;
            if (newCookie != oldCookie) {
                Logger.Debug($"OnCookieChanged newCookie={newCookie}");
                DataController.Default.SaveUserData(newCookie);
                await DataController.Default.Initialize();
                await RefreshDailyNote(null, null);
                await SchedulerController.Default.CheckSignReward("OnCookieChanged");
                Logger.Debug($"OnCookieChanged data saved");
            } else {
                Logger.Debug($"OnCookieChanged not change");
            }
        }

        private async void OnRefershButtonClicked(object sender, EventArgs e) {
            await RefreshDailyNote(sender, e);
        }

        private void OnCookieButtonClicked(object sender, EventArgs e) {
            if (IsRefreshingData) { return; }
            ShowCookieDialog();
        }

        private void OnOptionButtonClicked(object sender, EventArgs e) {
            if (IsRefreshingData) { return; }
            Logger.Debug($"OnOptionButtonClicked");
            var cd = new OptionForm {
                Location = new Point(this.Location.X + 80, this.Location.Y + 120),
                Owner = this
            };
            cd.ShowDialog();
        }

        private void UpdateRefreshState(bool refreshing) {
            IsRefreshingData = refreshing;
            this.RefreshButton.Enabled = !refreshing;
            this.CookieButton.Enabled = !refreshing;
            this.LoadingPic.Visible = refreshing;
        }

        private async Task RefreshDailyNote(object sender, EventArgs e) {
            Logger.Debug("RefreshDailyNote");
            if (IsRefreshingData) { return; }
            UpdateRefreshState(true);
            var user = DataController.Default.UserCached;
            var (note, _) = await DataController.Default.GetDailyNote();
            Logger.Debug($"RefreshDailyNote user={user?.GameUid} resin={note?.CurrentResin}");
            LastUpdateTime = DateTime.Now;
            UpdateUIControls(user, note);
            UpdateRefreshState(false);
        }

        private void UpdateUIControlsUseCache() {
            var user = DataController.Default.UserCached;
            var note = DataController.Default.NoteCached;
            UpdateUIControls(user, note);
        }

        private void UpdateUIControls(UserGameRole user, DailyNote note) {
            if (user == null || note == null) {
                Logger.Debug($"UpdateUIControls skip null data");
                return;
            }
            if (!this.Visible) {
                Logger.Debug($"UpdateUIControls skip hidden form");
                return;
            }
            Logger.Debug($"UpdateUIControls uid={user?.GameUid} resin={note?.CurrentResin}");

            StopCookieBlinkTimer();

            var colorNormal = Color.Green;
            var colorAttention = Color.Red;
            AccountValueL.Text = $"{user.Nickname} {user.Level}级 / {user.RegionName}({user.Server}) / {user.GameUid}";
            AccountValueL.ForeColor = Color.Blue;
            var resinMayFull = note.ResinAlmostFull();
            ResinValueL.Text = $"{note.CurrentResin}/{note.MaxResin}";
            ResinValueL.ForeColor = resinMayFull ? colorAttention : colorNormal;
            ResinRecValueL.Text = $"{note.ResinRecoveryTimeFormatted}";
            ResinRecValueL.ForeColor = resinMayFull ? colorAttention : colorNormal;
            ResinTimeValueL.Text = $"{note.ResinRecoveryTargetTimeFormatted}";
            ResinTimeValueL.ForeColor = resinMayFull ? colorAttention : colorNormal;

            var expeditionCompleted = note.ExpeditionAllFinished;
            var expeditionStr = $"{note.CurrentExpeditionNum}/{note.MaxExpeditionNum}";
            expeditionStr += expeditionCompleted ? " (已完成)" : " (未完成)";
            ExpeditionValueL.Text = expeditionStr;
            ExpeditionValueL.ForeColor = expeditionCompleted ? colorAttention : colorNormal;

            var taskStr = $"{note.FinishedTaskNum}/{note.TotalTaskNum}";
            if (!note.DailyTaskAllFinished) {
                taskStr += " (未完成)";
            } else {
                taskStr += note.IsExtraTaskRewardReceived ? " (已领取)" : " (未领取)";
            }
            TaskNameValueL.Text = taskStr;
            TaskNameValueL.ForeColor = note.IsExtraTaskRewardReceived ? colorNormal : colorAttention;

            var homeCoinMayFull = note.HomeCoinAlmostFull();
            HomeCoinValueL.Text = $"{note.CurrentHomeCoin}/{note.MaxHomeCoin}";
            HomeCoinValueL.ForeColor = homeCoinMayFull ? colorAttention : colorNormal;

            var discountAllUsed = note.ResinDiscountAllUsed;
            var discountStr = $"{note.ResinDiscountUsedNum}/{note.ResinDiscountNumLimit}";
            discountStr += discountAllUsed ? " (已完成)" : " (未完成)";
            DiscountTaskValueL.Text = discountStr;
            DiscountTaskValueL.ForeColor = discountAllUsed ? colorNormal : colorAttention;

            var transformerReady = note.TransformerReady;
            TransformerValueL.Text = $"{note.Transformer.RecoveryTime.TimeFormatted}";
            TransformerValueL.ForeColor = (transformerReady ? colorAttention : colorNormal);

            var updateDelta = DateTime.Now - note.CreatedAt;
            var outdated = updateDelta.TotalMinutes > 30;
            UpdatedValueL.Text = note.CreatedAt.ToString("T");
            UpdatedValueL.ForeColor = outdated ? colorAttention : colorNormal;
        }

        private void HideToTrayIcon() {
            this.Opacity = 0;
            //this.WindowState = FormWindowState.Normal;
            this.Hide();
            this.ShowInTaskbar = false;
            this.Opacity = 1;
            //CarrotNative.ShowWindow(this.Handle, CarrotNative.SW_HIDE);
        }

        private void RestoreFromTrayIcon() {
            Logger.Debug($"RestoreFromTrayIcon() visible={Visible} window={WindowState} thread={AppUtils.ThreadId}");
            //if (!this.Visible) {
            this.Opacity = 0;
            this.Show();
            this.Activate();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
            // 调整透明度，避免界面闪烁
            this.Opacity = 1;
            //CarrotNative.ShowWindow(this.Handle, CarrotNative.SW_SHOW);
            //NativeHelper.SetForegroundWindow(Handle);
            //}
        }

        private void OnSizeChanged(object sender, EventArgs e) {
            Logger.Debug($"OnSizeChanged {this.WindowState}");
            if (Settings.Default.OptionHideToTray) {
                if (this.WindowState == FormWindowState.Minimized) {
                    HideToTrayIcon();
                }
            }
        }

        private void AppNotifyIcon_DoubleClick(object sender, EventArgs e) {
            RestoreFromTrayIcon();
        }

        private void MenuItemShow_Click(object sender, EventArgs e) {
            RestoreFromTrayIcon();
        }

        private async void MenuItemCheckin_Click(object sender, EventArgs e) {
            Logger.Debug($"MenuItemCheckin_Click");
            await SchedulerController.Default.ManualSignReward();
        }

        private void MenuItemQuit_Click(object sender, EventArgs e) {
            Dispose();
            Close();
        }

        private void OnAccountLabelClicked(object sender, EventArgs e) {
#if DEBUG
            SchedulerController.Default.ShowNotification(DataController.Default.UserCached, DataController.Default.NoteCached);
#endif
        }
    }
}