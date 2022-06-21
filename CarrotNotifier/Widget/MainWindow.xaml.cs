using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using GenshinLib;
using GenshinNotifier.Properties;
using Carrot.Common.Native;
using Carrot.UI.Controls.Utils;
using System.Windows.Threading;
using System.Reflection;
using Carrot.Common;
using Newtonsoft.Json.Linq;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.System;
using System.Configuration;
using Carrot.UI.Controls.Dialog;
using System.Diagnostics.CodeAnalysis;
using System.Web.Services.Protocols;
using System.Security.Cryptography;

namespace GenshinNotifier {

    // https://stackoverflow.com/questions/12591896/disable-wpf-window-focus

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private double _dpiScale = 1;
        private IntPtr? _windowHandle;
        private HwndSource? _wndSource;
        private bool _windowSink = false;
        private double _lastPosX = double.NaN;
        private double _lastPosY = double.NaN;

        private WidgetViewModel _viewModel = DataController.Default.ViewModel;

        public MainWindow() {
            InitializeComponent();
            InitLocation();
            this.SourceInitialized += MainWindow_SourceInitialized;
            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += MainWindow_SizeChanged;
            this.LocationChanged += MainWindow_LocationChanged;
            this.Closing += MainWindow_Closing;
            this.Closed += MainWindow_Closed;
            this.Topmost = Settings.Default.OptionWidgetTopMost;
            Logger.Debug($"MainWindow lock={Settings.Default.OptionLockWidgetPos} top={Settings.Default.OptionWidgetTopMost}");
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            //Logger.Debug($"MainWindow_SizeChanged newSize={e.NewSize}");
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e) {
            //Logger.Debug($"MainWindow_LocationChanged {this.Left} {this.Top}");
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e) {
            var workAreaWidth = SystemParameters.WorkArea.Width;
            var workAreaHeight = SystemParameters.WorkArea.Height;
            var fixedX = MiscUtils.Clamp(this.Left, 0, workAreaWidth - this.Width - 10);
            var fixedY = MiscUtils.Clamp(this.Top, 0, workAreaHeight - this.Height - 10);
            Logger.Debug($"MainWindow_Closing {this.Left}->{fixedX} {this.Top}->{fixedY}");
            Settings.Default.WidgetLastPositionX = fixedX;
            Settings.Default.WidgetLastPositionY = fixedY;
            Settings.Default.Save();
        }

        private void MainWindow_Closed(object sender, EventArgs e) {
            Logger.Debug($"MainWindow_Closed {this.Left} {this.Top}");
            Settings.Default.PropertyChanged -= Settings_PropertyChanged;

            Settings.Default.Save();
            _wndSource?.RemoveHook(WndProc);
            _wndSource?.Dispose();
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            Logger.Debug($"Settings_PropertyChanged {e.PropertyName} = {Settings.Default[e.PropertyName]}");
            if (e.PropertyName == "OptionWidgetTopMost") {
                this.Topmost = Settings.Default.OptionWidgetTopMost;
            }
            if (e.PropertyName == "OptionLockWidgetPos") {
                // not used
            } else if (e.PropertyName == "MihoyoCookie") {
                // not used
            }
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e) {
            Logger.Debug($"MainWindow_SourceInitialized");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            //_dpiScale = this.GetDpiScale();
            //
            // https://tyrrrz.me/blog/wndproc-in-wpf
            // https://pingfu.net/receive-wndproc-messages-in-wpf
            //_windowHandle = new WindowInteropHelper(this).Handle;
            //_wndSource = HwndSource.FromHwnd(_windowHandle);
            //_wndSource.AddHook(WndProc);
            _wndSource = PresentationSource.FromVisual(this) as HwndSource;
            _dpiScale = _wndSource?.CompositionTarget.TransformToDevice.M11 ?? 1;
            _windowHandle = _wndSource?.Handle;
            _wndSource?.AddHook(WndProc);
            if (_windowHandle is IntPtr hwnd) {
                if (_windowSink) {
                    WindowUtils.SetCommonStyles(hwnd);
                    WindowUtils.ShowAlwaysOnDesktop(hwnd);
                    if (Environment.OSVersion.Version.Major >= 10) {
                        WindowUtils.ShowBehindDesktopIcons(hwnd);
                    }
                } else {
                    WindowUtils.MakeWindowSpecial(hwnd);
                }
            }

            Logger.Debug($"MainWindow_Loaded dpiScale={_dpiScale}");
            InitilizeStyles();
            SetLocation();
            this.DataContext = WidgetStyle.User;
            WidgetStyle.User.PropertyChanged += WidgetStyle_PropertyChanged;
            Settings.Default.PropertyChanged += Settings_PropertyChanged;
            DataController.Default.ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            Logger.Debug(DataController.Default.Ready ? "Ready" : "NotReady");
            if (!DataController.Default.Ready) {
                lbHeader.Style = FindResource("HeaderErrorStyle") as Style;
                lbHeader.Content = "请点击右键添加帐号";
                lbAccountInfo.Content = "无帐号或Cookie失效，请点击右键帐号管理";
                lbAccountInfo.Style = FindResource("FooterErrorStyle") as Style;
            }
            UpdateUIControls();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var key = e.PropertyName;
            Logger.Debug($"ViewModel_PropertyChanged key={key}");
            if (key == "Note") {
                Dispatcher.Invoke(() => UpdateUIControls());
            }

        }

        private void WidgetStyle_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var key = e.PropertyName ?? String.Empty;
            var value = sender?.GetType().GetProperty(name: key)?.GetValue(sender);
            Logger.Debug($"WidgetStyle_PropertyChanged {key} = {value}");
            // alternative method
            // https://stackoverflow.com/questions/20770173
            // var prop = TypeDescriptor.GetProperties(sender)[e.PropertyName];
            // Logger.Debug($"WidgetStyle_PropertyChanged other {prop.Name} = {prop.GetValue(sender)}");
        }

        private void InitLocation() {
            _lastPosX = Settings.Default.WidgetLastPositionX;
            _lastPosY = Settings.Default.WidgetLastPositionY;
            Logger.Debug($"InitLocation settings left={_lastPosX} top={_lastPosY}");
        }

        private void SetLocation() {
            var left = _lastPosX;
            var top = _lastPosY;
            if (double.IsNaN(left) || double.IsNaN(top)) {
                //var screenWidth = SystemParameters.PrimaryScreenWidth;
                //var screenHeight = SystemParameters.PrimaryScreenHeight;
                var workAreaWidth = SystemParameters.WorkArea.Width;
                var workAreaHeight = SystemParameters.WorkArea.Height;
                //var taskBarHeight = screenHeight - workAreaHeight;
                var windowWidth = this.Width;
                var windowHeight = this.Height;
                left = workAreaWidth - windowWidth - 10;
                top = (workAreaHeight - windowHeight) / 2;
                Settings.Default.WidgetLastPositionX = left;
                Settings.Default.WidgetLastPositionY = top;
            }
            Logger.Debug($"SetLocation to left={left} top={top} w={this.Width} h={this.Height}");
            this.Left = left;
            this.Top = top;
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            Logger.Debug("OnSourceInitialized");
        }

        private unsafe IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (!_windowSink) { return IntPtr.Zero; }
            var message = (WindowMessage)msg;
            switch (message) {
                case WindowMessage.WM_WINDOWPOSCHANGING:
                    var ox = this.Left;
                    var oy = this.Top;
                    var windowPos = Marshal.PtrToStructure<UnsafeTypes.WindowPos>(lParam);
                    //if (ox == windowPos.x && oy == windowPos.y) { break; }
                    Logger.Debug($"WM_WINDOWPOSCHANGING x={windowPos.x} y={windowPos.y} " +
                        $"cx={windowPos.cx} cy={windowPos.cy}");
                    //windowPos.hwndInsertAfter = new IntPtr(HWND_BOTTOM);
                    //windowPos.flags &= ~(uint)SWP_NOZORDER;
                    //handled = true;
                    break;

                case WindowMessage.WM_DPICHANGED:
                    var rc = (UnsafeTypes.RECT*)lParam.ToPointer();
                    Logger.Debug($"WM_DPICHANGED x={rc->Left} y={rc->Top} cx={rc->Width} cy={rc->Height}");
                    //SetWindowPos(_windowHandle, IntPtr.Zero, 0, 0, rc->Right, rc->Left, SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOZORDER);
                    break;

                case WindowMessage.WM_NCHITTEST:
                    Logger.Debug($"WM_NCHITTEST");
                    break;

                case WindowMessage.WM_NCLBUTTONDOWN:
                    Logger.Debug($"WM_NCLBUTTONDOWN");
                    break;

                default:
                    break;
            }
            Logger.Debug($"WndProc msg={message} wParam={wParam} lParam={lParam}");
            return IntPtr.Zero;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            var lockPos = Settings.Default.OptionLockWidgetPos;
            //Logger.Debug($"Window_MouseDown lockPos={lockPos} button={e.ChangedButton} " +
            //    $"state={e.ButtonState} count={e.ClickCount}");
#if DEBUG
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2) {
                //RefreshData(forceUpdate: true);
                Logger.Debug($"Dump Style: {WidgetStyle.User}");
                return;
            }
#endif
            if (lockPos) { return; }
            if (_windowHandle is IntPtr hwnd) {
                switch (e.ChangedButton) {
                    case MouseButton.Left:
                        //Point pt = e.GetPosition(lbHeader);
                        //if (pt.Y < lbHeader.ActualHeight) {
                        //    DragMove();
                        //}
                        UnsafeNative.ReleaseCapture();
                        UnsafeNative.SendMessage(hwnd, UnsafeTypes.WM_NCLBUTTONDOWN, UnsafeTypes.HT_CAPTION, 0);
                        break;

                    case MouseButton.Right:
                        break;
                }
            }
        }

        // we can drag and move window when window style = none
        private void LayoutHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            //base.OnMouseLeftButtonDown(e);
            if (Settings.Default.OptionLockWidgetPos) { return; }
            // https://stackoverflow.com/questions/867140/drag-a-wpf-form-around-the-desktop
            // https://stackoverflow.com/questions/1761854/cant-drag-and-move-a-wpf-form
            // https://stackoverflow.com/questions/7417739/
            this.DragMove();
        }

        #region update ui controls
        private Style? styleHeader;
        private Style? styleFooter;
        private Style? StyleTextNormal;
        private Style? styleTextHighlight;

        private void InitilizeStyles() {
            styleHeader = FindResource("HeaderStyle") as Style;
            styleFooter = FindResource("FooterStyle") as Style;
            StyleTextNormal = FindResource("GIStyleNormal") as Style;
            styleTextHighlight = FindResource("GIStyleHighlight") as Style;
        }

        private void UpdateUIControls(UserGameRole user, DailyNote note) {
            // https://levelup.gitconnected.com/5-ways-to-clone-an-object-in-c-d1374ec28efa
#if DEBUG
            //user = (UserGameRole)user.Clone();
            //user.GameUid = "100000001";
            //user.Level = 60;
            //user.Nickname = "大白猫";
            //note = (DailyNote)note.Clone();
            //note.FinishedTaskNum = 0;
            //note.CurrentHomeCoin = note.MaxHomeCoin;
            //note.RemainResinDiscountNum = 3;
            //note.IsExtraTaskRewardReceived = false;
#endif

            lbHeader.Style = styleHeader;
            lbHeader.Content = "原神实时便签";
#if DEBUG
            lbHeader.Content = "原神实时便签 DEBUG";
#endif

            lbAccountInfo.Style = styleFooter;
            lbAccountInfo.Content = $"{user.Nickname} {user.Level}级 {user.RegionName} {user.GameUid}";


            var styleNormal = StyleTextNormal;
            var styleHightlight = styleTextHighlight;

            // apply resin data
            var resinMayFull = note.ResinAlmostFull();
            lbResinValue.Content = $"{note.CurrentResin}/{note.MaxResin}";
            lbResinRecValue.Content = $"{note.ResinRecoveryTimeFormatted}";
            lbResinTimeValue.Content = $"{note.ResinRecoveryTargetTimeFormatted}";
            // apply resin style
            var resinStyle = resinMayFull ? styleHightlight : styleNormal;
            lbResin.Style = resinStyle;
            lbResinValue.Style = resinStyle;
            lbResinRec.Style = resinStyle;
            lbResinRecValue.Style = resinStyle;
            lbResinTime.Style = resinStyle;
            lbResinTimeValue.Style = resinStyle;

            // apply expedition data
            var expeditionCompleted = note.ExpeditionAllFinished;
            var expeditionStr = $"{note.CurrentExpeditionNum}/{note.MaxExpeditionNum}";
            expeditionStr = (expeditionCompleted ? "已完成 " : "未完成 ") + expeditionStr;
            lbExpeditionValue.Content = expeditionStr;
            // apply expedition style
            lbExpedition.Style = expeditionCompleted ? styleHightlight : styleNormal;
            lbExpeditionValue.Style = lbExpedition.Style;

            var taskStr = $"{note.FinishedTaskNum}/{note.TotalTaskNum}";
            if (!note.DailyTaskAllFinished) {
                taskStr = "未完成 " + taskStr;
            } else {
                taskStr = (note.IsExtraTaskRewardReceived ? "已领取 " : "未领取 ") + taskStr;
            }
            lbDailyTaskValue.Content = taskStr;

            lbDailyTask.Style = note.IsExtraTaskRewardReceived ? styleNormal : styleHightlight;
            lbDailyTaskValue.Style = lbDailyTask.Style;

            var homeCoinMayFull = note.HomeCoinAlmostFull();
            lbHomeCoinValue.Content = $"{note.CurrentHomeCoin}/{note.MaxHomeCoin}";
            lbHomeCoin.Style = homeCoinMayFull ? styleHightlight : styleNormal;
            lbHomeCoinValue.Style = lbHomeCoin.Style;

            var discountAllUsed = note.ResinDiscountAllUsed;
            var discountStr = $"{note.ResinDiscountUsedNum}/{note.ResinDiscountNumLimit}";
            discountStr = (discountAllUsed ? "已完成 " : "未完成 ") + discountStr;
            lbDiscountValue.Content = discountStr;
            lbDiscount.Style = discountAllUsed ? styleNormal : styleHightlight;
            lbDiscountValue.Style = lbDiscount.Style;

            var transformerReady = note.TransformerReady;
            lbTransformerValue.Content = $"{note.Transformer?.RecoveryTime?.TimeFormatted}";

            lbTransformer.Style = (transformerReady ? styleHightlight : styleNormal);
            lbTransformerValue.Style = lbTransformer.Style;

            var updateDelta = DateTime.Now - note.CreatedAt;
            var outdated = updateDelta.TotalMinutes > 30;
            lbUpdateAtValue.Content = note.CreatedAt.ToString("T");

            lbUpdateAt.Style = outdated ? styleHightlight : styleNormal;
            lbUpdateAtValue.Style = lbUpdateAt.Style;
        }


        private void UpdateUIControls() {
            if (_viewModel.User is UserGameRole user
                && _viewModel.Note is DailyNote note) {
                Debug.WriteLine($"UpdateUIControls uid={user.GameUid} " +
                    $"resin={note.CurrentResin}");
                UpdateUIControls(user!, note!);
            }
        }
        #endregion


        private void CxmItemRefresh_Click(object sender, RoutedEventArgs e) {
            Logger.Debug("CxmItemRefresh_Click");
            if (!this.IsLoaded) { return; }
            SchedulerController.Default.ForceRefresh();
        }

        private bool optionWindowVisible = false;
        private void CxmItemOption_Click(object sender, RoutedEventArgs e) {
            Logger.Debug("CxmItemOption_Click");
            if (!this.IsLoaded) { return; }
            if (!optionWindowVisible) {
                optionWindowVisible = true;
                OptionWindow option = new OptionWindow();
                option.Owner = this;
                option.WindowStartupLocation = WindowStartupLocation.Manual;

                var (left, top) = GetDialogPosition();
                option.Left = left;
                option.Top = top;
                option.ShowDialog();
                optionWindowVisible = false;
            }
        }

        private void CxmItemAbout_Click(object sender, RoutedEventArgs e) {
            Logger.Debug("CxmItemAbout_Click");
            if (!this.IsLoaded) { return; }
            Process.Start(AutoUpdater.ProjectUrl);
        }

        private (double left, double top) GetDialogPosition() {
            var sw = SystemParameters.WorkArea.Width;
            var sh = SystemParameters.WorkArea.Height;
            //var taskBarHeight = screenHeight - workAreaHeight;
            var windowWidth = this.Width;
            var windowHeight = this.Height;

            const int dialogWidth = 440;

            var _left = this.Left > sw / 2 ? this.Left - dialogWidth : this.Left + windowWidth;
            var _top = this.Top;
            if (_top > sh - windowHeight) {
                _top = sh - windowHeight;
            }
            var cx = MiscUtils.Clamp(_left, 0, sw - windowWidth);
            var cy = MiscUtils.Clamp(_top, 0, sh - windowHeight);
            return (cx, cy);
        }

        private void CxmItemRestart_Click(object sender, RoutedEventArgs e) {
            Process.Start(AppInfo.ExecutablePath);
            Application.Current.Shutdown();
        }

        private void CxmItemClose_Click(object sender, RoutedEventArgs e) {
            Logger.Debug("CxmItemClose_Click");
            var resultOk = MessageDialog.Show(this,
                "程序关闭后将无法在桌面展示小组件，也不能提供系统通知提醒，确定退出吗？", "退出确认");
            if (resultOk) {
                Application.Current.Shutdown();
            }
        }

        private void ShowCookieDialog() {
            var cd = new CookieDialog {
                Location = this.ToDrawingPoint(this.Left - 400, this.Top + 80),
            };
            var result = cd.ShowDialog();
            Logger.Debug($"ShowCookieDialog result={result} cookie={cd.NewCookie}");
            if (result == System.Windows.Forms.DialogResult.OK) {
                OnCookieChanged(cd.NewCookie, cd.NewUser);
            }
        }


        private void OnCookieChanged(string? newCookie, UserGameRole? newUser) {
            var oldCookie = DataController.Default.Cookie;
            Logger.Debug($"OnCookieChanged newCookie={newCookie} newUser={newUser?.GameUid}");
            if (string.IsNullOrWhiteSpace(newCookie)) {
                // clear cookie event
                DataController.Default.ClearUserData();
                // restart application
                Process.Start(AppInfo.ExecutablePath);
                Application.Current.Shutdown();
                return;
            }
            if (newCookie != oldCookie) {
                DataController.Default.SaveUserData(newCookie, newUser);
                SchedulerController.Default.CheckData("OnCookieChanged");
                Logger.Debug($"OnCookieChanged changed data saved");
            } else {
                Logger.Debug($"OnCookieChanged not changed");
            }
        }

        private void CxmItemAccount_Click(object sender, RoutedEventArgs e) {
            ShowCookieDialog();
        }

        private void CxmItemSettings_Click(object sender, RoutedEventArgs e) {
            MiscUtils.ShowSettingsDialog(this);
        }

        private void CxmItemHide_Click(object sender, RoutedEventArgs e) {
            this.Hide();
        }

        private void CxmItemCheckUpdate_Click(object sender, RoutedEventArgs e) {
            AutoUpdater.ShowUpdater();
        }
    }
}