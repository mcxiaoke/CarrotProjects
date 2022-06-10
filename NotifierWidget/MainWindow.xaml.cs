using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using static NotifierWidget.NativeMethods;
using NotifierWidget.Properties;
using GenshinLib;
using System.Timers;
using System.Collections;

namespace NotifierWidget {

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private bool _windowSink;
        private bool _lockWidgetPos;
        private bool _makeTopMost;
        private IntPtr _windowHandle;
        private HwndSource _wndSource;

        public MainWindow() {
            InitializeComponent();
            _lockWidgetPos = Settings.Default.OptionLockWidgetPos;
            _windowSink = Settings.Default.OptionWindowSink;
            _makeTopMost = Settings.Default.OptionTopMost;
            Settings.Default.PropertyChanged += Default_PropertyChanged;


            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
            //btnA.BorderBrush = System.Windows.Media.Brushes.Transparent;
            //btnA.BorderThickness = new Thickness(0);

            this.Topmost = _makeTopMost;
        }

        private void Default_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            Debug.WriteLine($"PropertyChanged {e.PropertyName} = {Settings.Default[e.PropertyName]}");
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine("MainWindow_Unloaded");
            _wndSource.RemoveHook(WndProc);
            _wndSource.Dispose();
            StopTimer();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine("MainWindow_Loaded");
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

            foreach (DictionaryEntry kvp in this.Resources) {
                Debug.WriteLine("{0} = {1} {2}", kvp.Key, kvp.Value, kvp.Value.GetType().Name);
            }

        }

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
                    Task.Delay(6000);
                }
                var data = Service.GetData();
                Debug.WriteLine($"RefreshData uid={data?.User?.GameUid} resin={data?.Note?.CurrentResin}");
                if (data == null || data.Note == null || data.User == null) {
                    return;
                }
                Dispatcher.Invoke(() => {
                    UpdateUIControls(data.User, data.Note);
                });
            });
        }

        private void UpdateUIControls(UserGameRole user, DailyNote note) {
            if (user == null || note == null) {
                Debug.WriteLine($"UpdateUIControls skip null data");
                return;
            }
            Debug.WriteLine($"UpdateUIControls uid={user?.GameUid} resin={note?.CurrentResin}");

            var colorNormal = UI.GetSolidColorBrush("#dee8f3");
            var colorAttention = Brushes.Yellow;
            lbAccountInfo.Content = $"{user.Nickname} {user.Level}级 {user.RegionName} {user.GameUid}";
            var resinMayFull = note.ResinAlmostFull();
            lbResinValue.Content = $"{note.CurrentResin}/{note.MaxResin}";
            lbResinValue.Foreground = resinMayFull ? colorAttention : colorNormal;
            lbResinRecValue.Content = $"{note.ResinRecoveryTimeFormatted}";
            lbResinRecValue.Foreground = resinMayFull ? colorAttention : colorNormal;
            lbResinTimeValue.Content = $"{note.ResinRecoveryTargetTimeFormatted}";
            lbResinTimeValue.Foreground = resinMayFull ? colorAttention : colorNormal;

            var expeditionCompleted = note.ExpeditionAllFinished;
            var expeditionStr = $"{note.CurrentExpeditionNum}/{note.MaxExpeditionNum}";
            expeditionStr += expeditionCompleted ? " 已完成" : " 未完成";
            lbExpeditionValue.Content = expeditionStr;
            lbExpeditionValue.Foreground = expeditionCompleted ? colorAttention : colorNormal;
            lbExpedition.Foreground = lbExpeditionValue.Foreground;

            var taskStr = $"{note.FinishedTaskNum}/{note.TotalTaskNum}";
            if (!note.DailyTaskAllFinished) {
                taskStr += " 未完成";
            } else {
                taskStr += note.IsExtraTaskRewardReceived ? " 已领取" : " 未领取";
            }
            lbDailyTaskValue.Content = taskStr;
            lbDailyTaskValue.Foreground = note.IsExtraTaskRewardReceived ? colorNormal : colorAttention;
            lbDailyTask.Foreground = lbDailyTaskValue.Foreground;

            var homeCoinMayFull = note.HomeCoinAlmostFull();
            lbHomeCoinValue.Content = $"{note.CurrentHomeCoin}/{note.MaxHomeCoin}";
            lbHomeCoinValue.Foreground = homeCoinMayFull ? colorAttention : colorNormal;

            var discountAllUsed = note.ResinDiscountAllUsed;
            var discountStr = $"{note.ResinDiscountUsedNum}/{note.ResinDiscountNumLimit}";
            discountStr += discountAllUsed ? " 已完成" : " 未完成";
            lbDiscountValue.Content = discountStr;
            lbDiscountValue.Foreground = discountAllUsed ? colorNormal : colorAttention;

            var transformerReady = note.TransformerReady;
            lbTransformerValue.Content = $"{note.Transformer.RecoveryTime.TimeFormatted}";
            lbTransformerValue.Foreground = (transformerReady ? colorAttention : colorNormal);

            var updateDelta = DateTime.Now - note.CreatedAt;
            var outdated = updateDelta.TotalMinutes > 30;
            lbUpdateAtValue.Content = note.CreatedAt.ToString("T");
            lbUpdateAtValue.Foreground = outdated ? colorAttention : colorNormal;
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            Debug.WriteLine("OnSourceInitialized");
        }

        unsafe IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
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
                    windowPos.hwndInsertAfter = new IntPtr(HWND_BOTTOM);
                    windowPos.flags &= ~(uint)SWP_NOZORDER;
                    handled = true;
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
            Debug.WriteLine($"Window_MouseDown {e.ChangedButton} {e.ButtonState} {e.ClickCount}");
            if (_lockWidgetPos) { return; }
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
            Close();
        }

        private void CxmItemLock_Checked(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemLock_Checked");
            _lockWidgetPos = true;
            Settings.Default.OptionLockWidgetPos = true;
            Settings.Default.Save();
        }

        private void CxmItemLock_Unchecked(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemLock_Unchecked");
            _lockWidgetPos = false;
            Settings.Default.OptionLockWidgetPos = false;
            Settings.Default.Save();
        }

        private void CxmItemRefresh_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemRefresh_Click");
            RefreshData(forceUpdate: true);
        }

        private void CxmItemOption_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemOption_Click");
        }

        private void CxmItemAbout_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemAbout_Click");
        }

        private void CxmItemTop_Checked(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemTop_Checked");
            Settings.Default.OptionTopMost = true;
            Settings.Default.Save();
            _makeTopMost = true;
            this.Topmost = true;
        }

        private void CxmItemTop_Unchecked(object sender, RoutedEventArgs e) {
            Debug.WriteLine("CxmItemTop_Unchecked");
            Settings.Default.OptionTopMost = false;
            Settings.Default.Save();
            _makeTopMost = false;
            this.Topmost = false;
        }
    }
}