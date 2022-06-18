using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media;

namespace Carrot.UI.Controls.Font {

    internal class AvailableColors : List<FontColor> {
        internal static readonly AvailableColors AllColors = new AvailableColors();

        #region Conversion Utils Static Methods

        public static FontColor GetFontColor(SolidColorBrush b) {
            return AllColors.GetFontColorByBrush(b);
        }

        public static FontColor GetFontColor(string name) {
            return AllColors.GetFontColorByName(name);
        }

        public static FontColor GetFontColor(Color c) {
            return AvailableColors.GetFontColor(new SolidColorBrush(c));
        }

        public static int GetFontColorIndex(FontColor c) {
            return AllColors.FindIndex(it => it.Brush.Color.Equals(c.Brush.Color));
        }

        #endregion Conversion Utils Static Methods

        public AvailableColors()
            : base() {
            Type brushesType = typeof(Colors);
            var properties = brushesType.GetProperties(BindingFlags.Static | BindingFlags.Public);

            foreach (var prop in properties) {
                string name = prop.Name;
                SolidColorBrush brush = new SolidColorBrush((Color)(prop.GetValue(null, null)));
                this.Add(new FontColor(name, brush));
            }
        }

        public FontColor GetFontColorByName(string name) {
            FontColor found = null;
            foreach (FontColor b in this) {
                if (b.Name == name) {
                    found = b;
                    break;
                }
            }
            return found;
        }

        public FontColor GetFontColorByBrush(SolidColorBrush b) {
            FontColor found = null;
            foreach (FontColor brush in this) {
                if (brush.Brush.Color.Equals(b.Color)) {
                    found = brush;
                    break;
                }
            }
            return found;
        }
    }
}