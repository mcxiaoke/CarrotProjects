using System.Windows;
using System.Windows.Media;

namespace Carrot.UI.Controls.Font {

    /// <summary>
    /// Interaction logic for ColorFontDialog.xaml
    /// </summary>
    public partial class ColorFontDialog : Window {
        private FontChooserInfo selectedFont;

        public ColorFontDialog() {
            this.selectedFont = null; // Default
            InitializeComponent();
        }

        public FontChooserInfo Font {
            get {
                return this.selectedFont;
            }

            set {
                FontChooserInfo fi = value;
                this.selectedFont = fi;
            }
        }

        public bool ShowColorPicker {
            get => this.colorFontChooser.ShowColorPicker;
            set => this.colorFontChooser.ShowColorPicker = value;
        }

        private void SyncFontName() {
            string fontFamilyName = this.selectedFont.Font.Name;
            int idx = 0;
            foreach (FontExtraInfo item in this.colorFontChooser.lstFamily.Items) {
                if (fontFamilyName == item.Name) {
                    break;
                }
                idx++;
            }
            this.colorFontChooser.lstFamily.SelectedIndex = idx;
            this.colorFontChooser.lstFamily.ScrollIntoView(this.colorFontChooser.lstFamily.Items[idx]);
        }

        private void SyncFontSize() {
            double fontSize = this.selectedFont.Size;
            this.colorFontChooser.fontSizeSlider.Value = fontSize;
        }

        private void SyncFontColor() {
            int colorIdx = AvailableColors.GetFontColorIndex(this.Font.Color);
            this.colorFontChooser.colorPicker.superCombo.SelectedIndex = colorIdx;
            this.colorFontChooser.txtSampleText.Foreground = this.Font.Color.Brush;
            this.colorFontChooser.colorPicker.superCombo.BringIntoView();
        }

        private void SyncFontTypeface() {
            string fontTypeFaceSb = FontChooserInfo.TypefaceToString(this.selectedFont.Typeface);
            int idx = 0;
            foreach (var item in this.colorFontChooser.lstTypefaces.Items) {
                FamilyTypeface face = item as FamilyTypeface;
                if (fontTypeFaceSb == FontChooserInfo.TypefaceToString(face)) {
                    break;
                }
                idx++;
            }
            this.colorFontChooser.lstTypefaces.SelectedIndex = idx;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) {
            this.Font = this.colorFontChooser.SelectedFont;
            this.DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.SyncFontColor();
            this.SyncFontName();
            this.SyncFontSize();
            this.SyncFontTypeface();
        }
    }
}