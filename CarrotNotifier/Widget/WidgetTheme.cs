using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Threading.Tasks;
using Carrot.UI.Controls.Utils;

namespace GenshinNotifier.Widget {
    public struct ThemeColor {

        public static ThemeColor Create(string bgHex, string normalHex, string highlightHex) {
            var bg = UIHelper.ParseColor(bgHex);
            var normal = UIHelper.ParseColor(normalHex);
            var highlight = UIHelper.ParseColor(highlightHex);
            return new ThemeColor(bg, normal, highlight);
        }

        public Color Background;
        public Color Normal;
        public Color Highlight;

        public ThemeColor(Color background, Color text, Color highlight) {
            Background = background;
            Normal = text;
            Highlight = highlight;
        }
    }

    public class WidgetTheme {
    }
}
