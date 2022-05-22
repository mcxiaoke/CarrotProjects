using System;
using System.Text;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinNotifier.Net;
using GenshinNotifier.Properties;
using System.ComponentModel;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;

namespace GenshinNotifier {
    public partial class MainForm : Form {

        private bool IsRefreshingData = false;
        private DateTime LastUpdateTime = DateTime.MinValue;
        private bool HideOnStart = false;

        public MainForm(bool shouldHide) {
            HideOnStart = shouldHide;
            InitializeComponent();
        }

        private void PrintAllSettings() {
            Logger.Verbose("---------- SETTINGS BEGIN ----------");
            var sb = new StringBuilder();
            foreach (SettingsProperty key in Settings.Default.Properties) {
                var value = Settings.Default[key.Name];
                sb.Append($"\n{key.Name} = {value}");
            }
            Logger.Verbose(sb.ToString());
            Logger.Verbose("---------- SETTINGS END ----------\n");
        }

        private async void OnFormLoad(object sender, EventArgs e) {
            Console.WriteLine("OnFormLoad()");
            PrintAllSettings();
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
                AccountValueL.Text = "当前Cookie为空或已失效，请设置Cookie后使用";
                AccountValueL.ForeColor = Color.Blue;
            }
        }

        private bool IsFormLoaded;
        async void OnFormShow(object sender, EventArgs e) {
            Console.WriteLine($"OnFormShow() hide={HideOnStart}");
            await CheckLocalAssets();
            UDPService.Handlers += OnNewInstance;
            SchedulerController.Default.Initialize();
            SchedulerController.Default.Handlers += OnDataUpdated;
            ToastNotificationManagerCompat.OnActivated += OnNotificationActivated;
            Settings.Default.PropertyChanged += OnSettingValueChanged;
            if (Settings.Default.FirstLaunch) {
                Settings.Default.FirstLaunch = false;
                Settings.Default.Save();
            }
            IsFormLoaded = true;
            if (DataController.Default.Ready) {
                StopCookieBlinkTimer();
            } else {
                StartCookieBlinkTimer();
            }
            if (HideOnStart) {
                HideToTrayIcon();
            }
        }

        private void OnNewInstance(object sender, EventArgs e) {
            Console.WriteLine($"OnNewInstance() {sender}");
            Invoke(new Action(() => {
                RestoreFromTrayIcon();
            }));
        }

        private System.Timers.Timer CookieBlinkTimer;
        private void StartCookieBlinkTimer() {
            //Logger.Debug("StartCookieBlinkTimer");
            CookieBlinkTimer = new System.Timers.Timer();
            CookieBlinkTimer.Interval = 250;
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

        private const string ICON_FILE_NAME = "carrot_512.png";
        private static readonly string IconFilePath = Path.Combine(Storage.UserDataFolder, "assets", ICON_FILE_NAME);
        private async Task CheckLocalAssets() {
            await Task.Run(() => {
                var assetsDir = Directory.GetParent(IconFilePath).FullName;
                Storage.CheckOrCreateDir(assetsDir);
                if (!File.Exists(IconFilePath)) {
                    Resources.ImageCarrot512.Save(IconFilePath);
                    Logger.Info($"CheckLocalAssets copied to {IconFilePath}");
                }
            });
        }

        private void ShowNotification() {
            // https://docs.microsoft.com/zh-cn/windows/apps/design/shell/tiles-and-notifications/adaptive-interactive-toasts?tabs=builder-syntax
            var image = IconFilePath;
            Logger.Debug(new Uri(image).AbsolutePath);
            var toast = new ToastContentBuilder()
                .SetToastScenario(ToastScenario.Reminder)
                .AddHeader("1", Storage.AppName, "action=open&id=1")
                .AddArgument("type", "resin")
                .AddArgument("action", "view")
                .AddArgument("conversationId", 1001)
                .AddText("Andrew sent you a picture")
                .AddText("Check this out 11, The Enchantments")
                .AddText("Check this out 33, The Enchantments ExpirationTime = DateTime.Now.AddMinutes(30)")
                .AddAttributionText(DateTime.Now.ToString("F"))
                .AddAppLogoOverride(new Uri(image), ToastGenericAppLogoCrop.Circle)
                // Buttons
                .AddButton(new ToastButton()
                    .SetContent("打开主界面")
                    .AddArgument("action", "show")
                    .SetBackgroundActivation())
                .AddButton(new ToastButton()
                .SetContent("今日不再提醒")
                .AddArgument("action", "mute")
                .SetBackgroundActivation());
            toast.Show(t => {
                t.Group = "Notification";
                t.Tag = "DailyNote";
                t.ExpirationTime = DateTime.Now.AddMinutes(30);
            });
        }

        private void OnNotificationActivated(ToastNotificationActivatedEventArgsCompat toastArgs) {
            Logger.Debug($"OnNotificationActivated {toastArgs.Argument}");
            // Obtain the arguments from the notification
            ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

            // Obtain any user input (text boxes, menu selections) from the notification
            ValueSet userInput = toastArgs.UserInput;

            // Need to dispatch to UI thread if performing UI operations
            Invoke(new Action(() => {
                Logger.Debug("Toast activated. Args: " + toastArgs.Argument);
            }));
        }

        private async void OnVisibleChanged(object sender, EventArgs e) {
            Console.WriteLine($"OnVisibleChanged() visible={this.Visible} formLoaded={IsFormLoaded}");
            if (!this.Visible) { return; }
            if (!IsFormLoaded) { return; }
            if (!DataController.Default.Ready) { return; }
            var user = DataController.Default.UserCached;
            var note = DataController.Default.NoteCached;
            Logger.Debug($"OnVisibleChanged update uid={user?.GameUid} resin={note?.CurrentResin}");
            UpdateUIControls(user, note);
            var needRefresh = note != null && (DateTime.Now - note.CreatedAt).TotalMinutes > 10;
            if (needRefresh) {
                await RefreshDailyNote(null, null);
            }
            ToastNotificationManagerCompat.History.Clear();
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) {
            Console.WriteLine("OnFormClosing()");
            StopCookieBlinkTimer();
            if (Settings.Default.OptionCloseConfirm) {
                if (e.CloseReason == CloseReason.UserClosing) {
                    var cd = new ConfirmDialog();
                    var ret = cd.ShowDialog();
                    //Logger.Debug($"ConfirmDialog {e.CloseReason} {ret}");
                    switch (ret) {
                        case DialogResult.Cancel:
                            // close button
                            e.Cancel = true;
                            break;
                        case DialogResult.No:
                            // minimize button
                            e.Cancel = true;
                            HideToTrayIcon();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e) {
            Console.WriteLine("OnFormClosed");
            IsFormLoaded = false;
            Settings.Default.PropertyChanged -= OnSettingValueChanged;
            SchedulerController.Default.Handlers -= OnDataUpdated;
            UDPService.Handlers -= OnNewInstance;
            NativeHelper.FreeConsole();
        }

        private void OnSettingValueChanged(object sender, PropertyChangedEventArgs e) {
            var key = e.PropertyName;
            var value = Settings.Default[key];
            Logger.Debug($"OnSettingValueChanged: {key}={value}");
            if (key == "OptionAutoStart") {
                Task.Run(() => ShortcutHelper.EnableAutoStart(Settings.Default.OptionAutoStart));
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
                DataController.Default.SaveUserData(newCookie);
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
            if (IsRefreshingData) { return; }
            ShowCookieDialog();
        }

        void OnOptionButtonClicked(object sender, EventArgs e) {
            if (IsRefreshingData) { return; }
            var cd = new OptionForm {
                Location = new Point(this.Location.X + 120, this.Location.Y + 80)
            };
            cd.ShowDialog();
        }

        void OnDataUpdated(UserGameRole user, DailyNote note) {
            Logger.Debug($"OnDataUpdated uid={user?.GameUid} resin={note?.CurrentResin} visible={this.Visible}");
            if (this.Visible) {
                UpdateUIControls(user, note);
            }
        }

        void UpdateRefreshState(bool refreshing) {
            IsRefreshingData = refreshing;
            this.RefreshButton.Enabled = !refreshing;
            this.CookieButton.Enabled = !refreshing;
            this.LoadingPic.Visible = refreshing;
        }


        async Task RefreshDailyNote(object sender, EventArgs e) {
            Logger.Debug("RefreshDailyNote");
            UpdateRefreshState(true);
            var (user, note) = await DataController.Default.GetDailyNote();
            Logger.Debug($"RefreshDailyNote user={user?.GameUid} resin={note?.CurrentResin}");
            UpdateUIControls(user, note);
            UpdateRefreshState(false);
        }

        void UpdateUIControls(UserGameRole user, DailyNote note) {
            if (user == null || note == null) {
                Logger.Debug($"UpdateUIControls skip null data");
                return;
            }
            Logger.Debug($"UpdateUIControls uid={user?.GameUid} resin={note?.CurrentResin}");

            StopCookieBlinkTimer();

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

            LastUpdateTime = DateTime.Now;
        }

        private bool HidePopupShown = false;
        private void HideToTrayIcon() {
            AppNotifyIcon.Visible = true;
            this.Opacity = 0;
            //this.WindowState = FormWindowState.Normal;
            this.Hide();
            this.ShowInTaskbar = false;
            if (!HidePopupShown && !HideOnStart) {
                HidePopupShown = true;
                AppNotifyIcon.ShowBalloonTip(1000, "已最小化到系统托盘", "双击图标恢复", ToolTipIcon.Info);
            }
        }

        private void RestoreFromTrayIcon() {
            Console.WriteLine($"RestoreFromTrayIcon() visible={Visible} window={WindowState}");
            if (!this.Visible) {
                this.Show();
                this.Activate();
                this.ShowInTaskbar = true;
                AppNotifyIcon.Visible = false;
            }
            this.WindowState = FormWindowState.Normal;
            // 调整透明度，避免界面闪烁
            this.Opacity = 1;
        }

        private void OnSizeChanged(object sender, EventArgs e) {
            Console.WriteLine($"OnSizeChanged {this.WindowState}");
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

        private void MenuItemCheckin_Click(object sender, EventArgs e) {

        }

        private void MenuItemQuit_Click(object sender, EventArgs e) {
            Dispose();
            Close();
        }

        private void OnAccountLabelClicked(object sender, EventArgs e) {
#if DEBUG
            ShowNotification();
#endif
        }
    }
}
