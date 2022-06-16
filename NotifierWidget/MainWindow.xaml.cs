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
using NotifierWidget.Properties;
using static NotifierWidget.NativeMethods;
using Carrot.UI.Controls.Utils;
using System.Windows.Threading;
using System.Reflection;
using CarrotCommon;

namespace NotifierWidget {

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private IntPtr _windowHandle;
        private HwndSource _wndSource;
        private bool _windowSink = false;
        private double _lastPosX;
        private double _lastPosY;

        public MainWindow() {
            InitializeComponent();
            InitLocation();
            this.Loaded += MainWindow_Loaded;
            this.SizeChanged += MainWindow_SizeChanged;
            this.ContentRendered += MainWindow_ContentRendered;
            this.LocationChanged += MainWindow_LocationChanged;
            this.Closing += MainWindow_Closing;
            this.Closed += MainWindow_Closed;
            // apply settings to ui
            this.cxmItemLock.IsChecked = Settings.Default.OptionLockWidgetPos;
            this.cxmItemTop.IsChecked = Settings.Default.OptionWidgetTopMost;
            this.Topmost = Settings.Default.OptionWidgetTopMost;
            Settings.Default.PropertyChanged += Default_PropertyChanged;
            Debug.WriteLine($"MainWindow lock={Settings.Default.OptionLockWidgetPos} top={Settings.Default.OptionWidgetTopMost}");
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
            Debug.WriteLine($"MainWindow_SizeChanged newSize={e.NewSize}");
        }

        private void MainWindow_ContentRendered(object sender, EventArgs e) {
            Debug.WriteLine("MainWindow_ContentRendered");
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e) {
            //Debug.WriteLine($"MainWindow_LocationChanged {this.Left} {this.Top}");
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e) {
            var workAreaWidth = SystemParameters.WorkArea.Width;
            var workAreaHeight = SystemParameters.WorkArea.Height;
            var fixedX = MiscUtils.Clamp(this.Left, 0, workAreaWidth - this.Width - 10);
            var fixedY = MiscUtils.Clamp(this.Top, 0, workAreaHeight - this.Height - 10);
            Debug.WriteLine($"MainWindow_Closing {this.Left}->{fixedX} {this.Top}->{fixedY}");
            Settings.Default.LastPositionX = fixedX;
            Settings.Default.LastPositionY = fixedY;
            Settings.Default.Save();
        }

        private void MainWindow_Closed(object sender, EventArgs e) {
            Debug.WriteLine($"MainWindow_Closed {this.Left} {this.Top}");
            _wndSource.RemoveHook(WndProc);
            _wndSource.Dispose();
            StopTimer();
        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            Debug.WriteLine($"PropertyChanged {e.PropertyName} = {Settings.Default[e.PropertyName]} {this.IsLoaded} {this.IsVisible}");
            if (e.PropertyName == "OptionWidgetTopMost") {
                this.Topmost = Settings.Default.OptionWidgetTopMost;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine("MainWindow_Loaded");
            this.DataContext = WidgetStyle.User;
            WidgetStyle.User.PropertyChanged += WidgetStyle_PropertyChanged;
            SetLocation();
            // https://tyrrrz.me/blog/wndproc-in-wpf
            // https://pingfu.net/receive-wndproc-messages-in-wpf
            //_windowHandle = new WindowInteropHelper(this).Handle;
            //_wndSource = HwndSource.FromHwnd(_windowHandle);
            //_wndSource.AddHook(WndProc);
            _wndSource = PresentationSource.FromVisual(this) as HwndSource;
            _windowHandle = _wndSource.Handle;
            _wndSource.AddHook(WndProc);
            if (_windowSink) {
                WindowUtils.SetCommonStyles(_windowHandle);
                WindowUtils.ShowAlwaysOnDesktop(_windowHandle);
                if (Environment.OSVersion.Version.Major >= 10) {
                    WindowUtils.ShowBehindDesktopIcons(_windowHandle);
                }
            }
            RefreshData();
            StartTimer();
        }

        private static Action EmptyDelegate = () => { };
        private void WidgetStyle_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            var key = e.PropertyName;
            var value = sender.GetType().GetProperty(key).GetValue(sender);
            Debug.WriteLine($"WidgetStyle_PropertyChanged {key} = {value}");
            // alternative method
            // https://stackoverflow.com/questions/20770173
            // var prop = TypeDescriptor.GetProperties(sender)[e.PropertyName];
            // Debug.WriteLine($"WidgetStyle_PropertyChanged other {prop.Name} = {prop.GetValue(sender)}");
            //this.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        private void InitLocation() {
            _lastPosX = Settings.Default.LastPositionX;
            _lastPosY = Settings.Default.LastPositionY;
            Debug.WriteLine($"InitLocation settings left={_lastPosX} top={_lastPosY}");
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
                Settings.Default.LastPositionX = left;
                Settings.Default.LastPositionY = top;
            }
            Debug.WriteLine($"SetLocation to left={left} top={top} w={this.Width} h={this.Height}");
            this.Left = left;
            this.Top = top;
        }

        #region timer and data refresh

        private const int TIME_ONE_SECOND_MS = 1000;
        private const int TIME_ONE_MINUTE_MS = 60 * TIME_ONE_SECOND_MS;
        private System.Timers.Timer refreshTimer;

        private void StartTimer() {
            StopTimer();
            Debug.WriteLine($"StartTimer");
            refreshTimer = new System.Timers.Timer {
                Interval = TIME_ONE_MINUTE_MS
            };
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.AutoReset = true;
            refreshTimer.Enabled = true;
        }

        private void StopTimer() {
            if (refreshTimer != null) {
                Debug.WriteLine($"StopTimer");
                refreshTimer.Stop();
                refreshTimer.Dispose();
            }
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e) {
            RefreshData();
        }

        private void RefreshData(bool forceUpdate = false) {
            Debug.WriteLine($"RefreshData {forceUpdate}");
            Task.Run(() => {
                if (forceUpdate) {
                    Service.Refresh();
                    Task.Delay(10000);
                }
                var data = Service.GetData();
                Debug.WriteLine($"RefreshData uid={data?.User?.GameUid} resin={data?.Note?.CurrentResin}");
                if (data == null || data.Note == null || data.User == null) {
                    return;
                }
                Dispatcher.Invoke(() => UpdateUIControls(data.User, data.Note));
            });
        }

        private void UpdateUIControls(UserGameRole user, DailyNote note) {
            if (user == null || note == null) {
                Debug.WriteLine($"UpdateUIControls skip null data");
                return;
            }
            Debug.WriteLine($"UpdateUIControls uid={user?.GameUid} resin={note?.CurrentResin}");

            var styleNormal = FindResource("GIStyleNormal") as Style;
            var styleHightlight = FindResource("GIStyleHighlight") as Style;

            lbAccountInfo.Content = $"{user.Nickname} {user.Level}级 {user.RegionName} {user.GameUid}";

            // apply resin data
            var resinMayFull = note.ResinAlmostFull();
            lbResinValue.Content = $"{note.CurrentResin}/{note.MaxResin}";
            lbResinRecValue.Content = $"{note.ResinRecoveryTimeFormatted}";
            lbResinTimeValue.Content = $"{note.ResinRecoveryTargetTimeFormatted}";
            // apply resin style
            var resinStyle = resinMayFull ? styleHightlight : styleNormal;
#if DEBUG
            //resinStyle = styleHightlight;
#endif
            lbResin.Style = resinStyle;
            lbResinValue.Style = resinStyle;
            lbResinRec.Style = resinStyle;
            lbResinRecValue.Style = resinStyle;
            lbResinTime.Style = resinStyle;
            lbResinTimeValue.Style = resinStyle;

            // apply expedition data
            var expeditionCompleted = note.ExpeditionAllFinished;
            var expeditionStr = $"{note.CurrentExpeditionNum}/{note.MaxExpeditionNum}";
            expeditionStr += expeditionCompleted ? " 已完成" : " 未完成";
            lbExpeditionValue.Content = expeditionStr;
            // apply expedition style
            lbExpedition.Style = expeditionCompleted ? styleHightlight : styleNormal;
            lbExpeditionValue.Style = lbExpedition.Style;

            var taskStr = $"{note.FinishedTaskNum}/{note.TotalTaskNum}";
            if (!note.DailyTaskAllFinished) {
                taskStr += " 未完成";
            } else {
                taskStr += note.IsExtraTaskRewardReceived ? " 已领取" : " 未领取";
            }
            lbDailyTaskValue.Content = taskStr;

            lbDailyTask.Style = note.IsExtraTaskRewardReceived ? styleNormal : styleHightlight;
            lbDailyTaskValue.Style = lbDailyTask.Style;

            var homeCoinMayFull = note.HomeCoinAlmostFull();
            lbHomeCoinValue.Content = $"{note.CurrentHomeCoin}/{note.MaxHomeCoin}";
            lbHomeCoinValue.Style = homeCoinMayFull ? styleHightlight : styleNormal;

            var discountAllUsed = note.ResinDiscountAllUsed;
            var discountStr = $"{note.ResinDiscountUsedNum}/{note.ResinDiscountNumLimit}";
            discountStr += discountAllUsed ? " 已完成" : " 未完成";
            lbDiscountValue.Content = discountStr;
            lbDiscount.Style = discountAllUsed ? styleNormal : styleHightlight;
            lbDiscountValue.Style = lbDiscount.Style;

            var transformerReady = note.TransformerReady;
            lbTransformerValue.Content = $"{note.Transformer.RecoveryTime.TimeFormatted}";

            lbTransformer.Style = (transformerReady ? styleHightlight : styleNormal);
            lbTransformerValue.Style = lbTransformer.Style;

            var updateDelta = DateTime.Now - note.CreatedAt;
            var outdated = updateDelta.TotalMinutes > 30;
            lbUpdateAtValue.Content = note.CreatedAt.ToString("T");

            lbUpdateAt.Style = outdated ? styleHightlight : styleNormal;
            lbUpdateAtValue.Style = lbUpdateAt.Style;
        }

        #endregion timer and data refresh

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            Debug.WriteLine("OnSourceInitialized");
        }

        private unsafe IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (!_windowSink) { return IntPtr.Zero; }
            var message = (WindowMessage)msg;
            switch (message) {
                case WindowMessage.WM_WINDOWPOSCHANGING:
                    var ox = this.Left;
                    var oy = this.Top;
                    var windowPos = Marshal.PtrToStructure<WindowPos>(lParam);
                    //if (ox == windowPos.x && oy == windowPos.y) { break; }
                    Debug.WriteLine($"WM_WINDOWPOSCHANGING x={windowPos.x} y={windowPos.y} " +
                        $"cx={windowPos.cx} cy={windowPos.cy}");
                    //windowPos.hwndInsertAfter = new IntPtr(HWND_BOTTOM);
                    //windowPos.flags &= ~(uint)SWP_NOZORDER;
                    //handled = true;
                    break;

                case WindowMessage.WM_DPICHANGED:
                    var rc = (RECT*)lParam.ToPointer();
                    Debug.WriteLine($"WM_DPICHANGED x={rc->Left} y={rc->Top} cx={rc->Width} cy={rc->Height}");
                    //SetWindowPos(_windowHandle, IntPtr.Zero, 0, 0, rc->Right, rc->Left, SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOZORDER);
                    break;

                case WindowMessage.WM_NCHITTEST:
                    Debug.WriteLine($"WM_NCHITTEST");
                    break;

                case WindowMessage.WM_NCLBUTTONDOWN:
                    Debug.WriteLine($"WM_NCLBUTTONDOWN");
                    break;

                default:
                    break;
            }
            //Debug.WriteLine($"WndProc msg={message} wParam={wParam} lParam={lParam}");
            return IntPtr.Zero;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            var lockPos = Settings.Default.OptionLockWidgetPos;
            Debug.WriteLine($"Window_MouseDown lockPos={lockPos} button={e.ChangedButton} " +
                $"state={e.ButtonState} count={e.ClickCount}");
#if DEBUG
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2) {
                RefreshData(forceUpdate: true);
                return;
            }
#endif
            if (lockPos) { return; }
            switch (e.ChangedButton) {
                case MouseButton.Left:
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(_windowHandle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HT_CAPTION, 0);
                    break;

                case MouseButton.Right:
                    break;
            }
        }

        private void CxmItemClose_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemClose_Click");
            if (MessageBox.Show("确定不需要桌面小组件，彻底退出吗？", "退出确认",
                MessageBoxButton.OKCancel) == MessageBoxResult.OK) {
                Close();
            }
        }

        private void CxmItemRefresh_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemRefresh_Click");
            if (!this.IsLoaded) { return; }
            RefreshData(forceUpdate: true);
        }

        private void CxmItemOption_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemOption_Click");
            if (!this.IsLoaded) { return; }
            OptionWindow option = new OptionWindow();
            option.Owner = this;
            option.WindowStartupLocation = WindowStartupLocation.Manual;

            GetDialogPosition(out double left, out double top);
            option.Left = left;
            option.Top = top;
            option.Show();
        }

        private void CxmItemAbout_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemAbout_Click");
            if (!this.IsLoaded) { return; }
            Process.Start("https://gitee.com/osap/CarrotProjects");
        }

        private void GetDialogPosition(out double left, out double top) {
            var workAreaWidth = SystemParameters.WorkArea.Width;
            var workAreaHeight = SystemParameters.WorkArea.Height;
            //var taskBarHeight = screenHeight - workAreaHeight;
            var windowWidth = this.Width;
            var windowHeight = this.Height;

            var _left = this.Left - 440;
            var _top = this.Top;
            if (_top > workAreaHeight - 420) {
                _top = workAreaHeight - 420;
            }
            left = _left;
            top = _top;
        }

        private void CxmItemRestart_Click(object sender, RoutedEventArgs e) {
            //ProcessStartInfo Info = new ProcessStartInfo();
            //Info.Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetEntryAssembly().Location + "\"";
            //Info.WindowStyle = ProcessWindowStyle.Hidden;
            //Info.CreateNoWindow = true;
            //Info.FileName = "cmd.exe";
            //Process.Start(Info);
            //Process.GetCurrentProcess().Kill();
            Process.Start(AppInfo.ExecutablePath);
            Application.Current.Shutdown();
        }
    }
}