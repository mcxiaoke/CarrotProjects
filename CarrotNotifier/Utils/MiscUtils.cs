using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Carrot.Common;

namespace GenshinNotifier {

    public static class MiscUtils {

        public static T Clamp<T>(T value, T min, T max)
where T : System.IComparable<T> {
            T result = value;
            if (value.CompareTo(max) > 0)
                result = max;
            if (value.CompareTo(min) < 0)
                result = min;
            return result;
        }

        public static void RestartApplication() {
            // https://stackoverflow.com/questions/1672338
            ProcessStartInfo Info = new ProcessStartInfo();
            // timeout hack for restarting app with mutex single instance check
            Info.Arguments = "/C choice /C Y /N /D Y /T 2 & START \"\" \"" + AppInfo.ExecutablePath + "\"";

            //Info.Arguments = "/C TIMEOUT 2 > nul & START \"\" \"" + AppInfo.ExecutablePath + "\"";
            Info.WindowStyle = ProcessWindowStyle.Hidden;
            Info.CreateNoWindow = true;
            Info.FileName = "cmd.exe";
            Process.Start(Info);
            //Process.GetCurrentProcess().Kill();
            Application.Current.Shutdown();
        }


        public static double GetDpiScale(this Window window) {
            // https://dzimchuk.net/best-way-to-get-dpi-value-in-wpf/
            // https://docs.microsoft.com/zh-cn/archive/blogs/jaimer/getting-system-dpi-in-wpf-app
            // https://stackoverflow.com/questions/1918877/how-can-i-get-the-dpi-in-wpf
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            //var dpiX = (int)dpiXProperty.GetValue(null, null);
            //var dpiY = (int)dpiYProperty.GetValue(null, null);
            //using (System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero)) {
            //    Debug.WriteLine($"Graphics x={g.DpiX} y={g.DpiY} s={g.DpiX / 96}");
            //    double factor = g.DpiX / 96;
            //}
            return PresentationSource.FromVisual(window).CompositionTarget.TransformToDevice.M11;
            // or
            // return VisualTreeHelper.GetDpi(window).DpiScaleX;

        }

        public static System.Drawing.Point ToDrawingPoint(this Window window, double x, double y) {
            var sw = SystemParameters.WorkArea.Width;
            var sh = SystemParameters.WorkArea.Height;
            var cx = MiscUtils.Clamp(x, 0, sw - window.Width);
            var cy = MiscUtils.Clamp(y, 0, sh - window.Height);
            return new System.Drawing.Point(Convert.ToInt32(cx * window.GetDpiScale()), Convert.ToInt32(cy * window.GetDpiScale()));
        }

        public static void ShowSettingsDialog(Window window) {
            var point = new System.Windows.Point(window.Left, window.Top);
            var transform = PresentationSource.FromVisual(window).CompositionTarget.TransformFromDevice;
            var cd = new OptionForm {
                Location = window.ToDrawingPoint(window.Left - window.Width, window.Top + 80),
            };
            cd.ShowDialog();
        }

    }


}