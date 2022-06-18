using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Carrot.UI.Controls;
using Carrot.UI.Controls.Utils;
using Carrot.UI.Controls.Picker;
using Carrot.UI.Controls.Font;
using Carrot.UI.ColorPicker;

namespace Carrot.UI.Samples {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window {
        public List<ColorComboBoxItem> TestColors => new List<ColorComboBoxItem>() {
             ColorComboBoxItem.Create("TestColor111",
                UIHelper.ParseColor("#334455")),
             ColorComboBoxItem.Create("TestColor211",
                UIHelper.ParseColor("#6688aa")),
        };

        public List<ColorComboBoxItem> _extraColors2 = new List<ColorComboBoxItem>() {
             ColorComboBoxItem.Create("TestColor133",
                UIHelper.ParseColor("#123456")),
             ColorComboBoxItem.Create("TestColor244",
                UIHelper.ParseColor("#80cccccc")),
        };

        public ICollection<FontExtraInfo> SystemFonts => FontUtilities.AllFonts;
        public FontExtraInfo RandomFont => SystemFonts.Skip(new Random().Next(20)).FirstOrDefault();


        public MainWindow() {
            InitializeComponent();
            Debug.WriteLine($"MainWindow init {RandomFont}");
        }

        private void ColorBox_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<ColorComboBoxItem> e) {
            Debug.WriteLine($"ColorBox_SelectedColorChanged changed {e.OldValue} => {e.NewValue}");
        }

        private void FontComboBox_FontChanged(object sender, RoutedEventArgs e) {

        }

        private void FontComboBox_FontChanged(object sender, RoutedPropertyChangedEventArgs<FontExtraInfo> e) {
            Debug.WriteLine($"FontComboBox_FontChanged old={e.OldValue} new={e.NewValue}");

        }

        private void FontButton_Click(object sender, RoutedEventArgs e) {
            ColorFontDialog fntDialog = new ColorFontDialog();
            fntDialog.ShowColorPicker = true;
            fntDialog.Owner = this;
            fntDialog.Font = FontChooserInfo.GetControlFont(this.textBox);
            if (fntDialog.ShowDialog() == true) {
                FontChooserInfo selectedFont = fntDialog.Font;

                if (selectedFont != null) {
                    FontChooserInfo.ApplyFont(this.textBox, selectedFont);
                }
            }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e) {
            var dialog = new ColorPickerDialog(Colors.BlueViolet) {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Title = "选择颜色",
                ButtonOKText = "确定",
                ButtonCancelText = "取消"
            };
            if (dialog.ShowDialog() == true) {
                textBox.Foreground = new SolidColorBrush(dialog.PickedColor);
            }
        }
    }
}
