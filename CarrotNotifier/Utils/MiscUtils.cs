﻿using System;
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

    }


}