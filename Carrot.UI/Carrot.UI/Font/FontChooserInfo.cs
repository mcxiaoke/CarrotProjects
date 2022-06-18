using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Carrot.UI.Controls.Font {

    public class FontChooserInfo {
        public FontExtraInfo Font { get; set; }
        public double Size { get; set; }
        public FontStyle Style { get; set; }
        public FontStretch Stretch { get; set; }
        public FontWeight Weight { get; set; }
        public SolidColorBrush BrushColor { get; set; }

        #region Static Utils

        public static string TypefaceToString(FamilyTypeface ttf) {
            StringBuilder sb = new StringBuilder(ttf.Stretch.ToString());
            sb.Append('-');
            sb.Append(ttf.Weight.ToString());
            sb.Append('-');
            sb.Append(ttf.Style.ToString());
            return sb.ToString();
        }

        public static void ApplyFont(Control control, FontChooserInfo font) {
            control.FontFamily = font.Font.Family;
            control.FontSize = font.Size;
            control.FontStyle = font.Style;
            control.FontStretch = font.Stretch;
            control.FontWeight = font.Weight;
            control.Foreground = font.BrushColor;
        }

        public static FontChooserInfo GetControlFont(Control control) {
            FontChooserInfo font = new FontChooserInfo();
            font.Font = FontUtilities.GetLocalizedFontFamily(control.FontFamily);
            font.Size = control.FontSize;
            font.Style = control.FontStyle;
            font.Stretch = control.FontStretch;
            font.Weight = control.FontWeight;
            font.BrushColor = (SolidColorBrush)control.Foreground;
            return font;
        }

        #endregion Static Utils

        public FontChooserInfo() {
        }

        public FontChooserInfo(FontFamily fam, double sz, FontStyle style,
                        FontStretch strc, FontWeight weight, SolidColorBrush c) {
            this.Font = FontUtilities.GetLocalizedFontFamily(fam);
            this.Size = sz;
            this.Style = style;
            this.Stretch = strc;
            this.Weight = weight;
            this.BrushColor = c;
        }

        public FontColor Color {
            get {
                return AvailableColors.GetFontColor(this.BrushColor);
            }
        }

        public FamilyTypeface Typeface {
            get {
                FamilyTypeface ftf = new FamilyTypeface();
                ftf.Stretch = this.Stretch;
                ftf.Weight = this.Weight;
                ftf.Style = this.Style;
                return ftf;
            }
        }

        public override string ToString() {
            return $"{{{nameof(Font)}={Font}, {nameof(Size)}={Size}, {nameof(Style)}={Style}, {nameof(Stretch)}={Stretch}, {nameof(Weight)}={Weight}, {nameof(BrushColor)}={BrushColor}, {nameof(Color)}={Color}, {nameof(Typeface)}={Typeface}}}";
        }
    }
}