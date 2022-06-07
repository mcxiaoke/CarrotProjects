using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace NotifierWidget {

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        private IntPtr _windowHandle;
        private HwndSource _wndSource;

        public MainWindow() {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
            btnA.BorderBrush = System.Windows.Media.Brushes.Transparent;
            btnA.BorderThickness = new Thickness(0);
        }

        private void MainWindow_Unloaded(object sender, RoutedEventArgs e) {
            //_wndSource.RemoveHook(WndProc);
            _wndSource.Dispose();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            // https://tyrrrz.me/blog/wndproc-in-wpf
            //_windowHandle = new WindowInteropHelper(this).Handle;
            //_wndSource = HwndSource.FromHwnd(_windowHandle);
            //_wndSource.AddHook(WndProc);
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);
            // https://pingfu.net/receive-wndproc-messages-in-wpf
            _wndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (_wndSource != null) {
                _windowHandle = _wndSource.Handle;
                //_wndSource.AddHook(WndProc);
            }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            var message = (WindowMessage)msg;
            var subCode = (WindowMessageParameter)wParam.ToInt32();

            //Debug.WriteLine($"WndProc msg={message} subCode={subCode} lParam={lParam}");

            return IntPtr.Zero;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            switch (e.ChangedButton) {
                case MouseButton.Left:
                    User32.ReleaseCapture();
                    User32.SendMessage(_windowHandle, User32.WM_NCLBUTTONDOWN, User32.HT_CAPTION, 0);
                    break;

                case MouseButton.Right:
                    break;
            }
        }

        protected override void OnRender(DrawingContext drawingContext) {
            base.OnRender(drawingContext);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}