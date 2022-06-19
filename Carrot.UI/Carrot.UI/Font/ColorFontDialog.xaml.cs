using System.Windows;
using System.Windows.Media;

namespace Carrot.UI.Controls.Font {

    /// <summary>
    /// Interaction logic for ColorFontDialog.xaml
    /// </summary>
    public partial class ColorFontDialog : Window {
        private FontChooserInfo? selectedFont;

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
            if (this.selectedFont is FontChooserInfo font) {
                string fontFamilyName = font.Font.Name;
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
        }

        private void SyncFontSize() {
            if (this.selectedFont is FontChooserInfo font) {
                this.colorFontChooser.fontSizeSlider.Value = font.Size;
            }
        }

        private void SyncFontColor() {
            int colorIdx = AvailableColors.GetFontColorIndex(this.Font.Color);
            this.colorFontChooser.colorPicker.superCombo.SelectedIndex = colorIdx;
            this.colorFontChooser.txtSampleText.Foreground = this.Font.Color.Brush;
            this.colorFontChooser.colorPicker.superCombo.BringIntoView();
        }

        private void SyncFontTypeface() {

            if (this.selectedFont is FontChooserInfo font) {

                string fontTypeFaceSb = FontChooserInfo.TypefaceToString(font.Typeface);
                int idx = 0;
                foreach (var item in this.colorFontChooser.lstTypefaces.Items) {
                    if (item is FamilyTypeface face
                        && fontTypeFaceSb == FontChooserInfo.TypefaceToString(face)) {
                        break;
                    }
                    idx++;
                }
                this.colorFontChooser.lstTypefaces.SelectedIndex = idx;
            }


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