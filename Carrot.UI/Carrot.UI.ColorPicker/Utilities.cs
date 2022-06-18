using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Carrot.UI.ColorPicker {
    internal static class Utilities {

        public const string User32 = "user32.dll";
        public const string Shcore = "shcore.dll";
        public const string D2D1 = "d2d1.dll";

        [DllImport(User32, ExactSpelling = true, CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetCursorPos([In][Out] POINT pt);

        [StructLayout(LayoutKind.Sequential)]
        public class POINT {
            public int x;
            public int y;

            public POINT() {
            }

            public POINT(int x, int y) {
                this.x = x;
                this.y = y;
            }

        }

        public static Point MouseCursorPos {
            get {
                var pt = new POINT();
                GetCursorPos(pt);
                return new Point(pt.x, pt.y);
            }
        }

    }
}
