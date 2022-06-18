using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;

namespace Carrot.UI.Controls.Font {

    public partial class ColorFontChooser : UserControl {


        public static readonly DependencyProperty ShowColorPickerProperty = DependencyProperty.Register(
    nameof(ShowColorPicker), typeof(bool), typeof(ColorFontChooser), new UIPropertyMetadata(true));

        public bool ShowColorPicker {
            get => (bool)GetValue(ShowColorPickerProperty);
            set {
                SetValue(ShowColorPickerProperty, value);
                colorPickerLayout.Visibility = ColorPickerVisibility;
            }
        }

        public Visibility ColorPickerVisibility => ShowColorPicker ? Visibility.Visible : Visibility.Collapsed;

        public FontChooserInfo SelectedFont {
            get {
                return new FontChooserInfo(this.txtSampleText.FontFamily,
                                    this.txtSampleText.FontSize,
                                    this.txtSampleText.FontStyle,
                                    this.txtSampleText.FontStretch,
                                    this.txtSampleText.FontWeight,
                                    this.colorPicker.SelectedColor.Brush);
            }
        }

        public ColorFontChooser() {
            InitializeComponent();
            this.DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine($"ColorFontChooser loaded {ShowColorPicker}");
            Debug.WriteLine($"ColorFontChooser loaded {ColorPickerVisibility}");
            //colorPickerLayout.Visibility = ColorPickerVisibility;
        }

        private void ColorPicker_ColorChanged(object sender, RoutedEventArgs e) {
            this.txtSampleText.Foreground = this.colorPicker.SelectedColor.Brush;
        }


    }
}