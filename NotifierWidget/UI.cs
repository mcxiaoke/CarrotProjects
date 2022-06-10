using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NotifierWidget {
    internal static class UI {

        public static T GetChildOfType<T>(this DependencyObject depObj) where T :
            DependencyObject {
            if (depObj == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static IEnumerable<T> FindLogicalChildren<T>(this DependencyObject parent) where T : DependencyObject {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            var queue = new Queue<DependencyObject>(new[] { parent });

            while (queue.Any()) {
                var reference = queue.Dequeue();
                var children = LogicalTreeHelper.GetChildren(reference);
                var objects = children.OfType<DependencyObject>();

                foreach (var o in objects) {
                    if (o is T child)
                        yield return child;

                    queue.Enqueue(o);
                }
            }
        }


        public static Color ParseColor(string hex) {
            hex = hex.Replace("#", string.Empty);
            if (hex.Length == 6) {
                hex = "FF" + hex;
            }
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            return Color.FromArgb(a, r, g, b);
        }

        public static Color ParseColor2(string hex) {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        public static SolidColorBrush GetSolidColorBrush(string hex) {
            return new SolidColorBrush(ParseColor(hex));
        }

        public static SolidColorBrush GetSolidColorBrush2(string hex) {
            return (SolidColorBrush)new BrushConverter().ConvertFrom(hex);
        }
    }
}
