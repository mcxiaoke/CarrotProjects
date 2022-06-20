using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
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
        public static List<NamedColor> TestColors => new List<NamedColor>() {
             NamedColor.Create("TestColor111",
                UIHelper.ParseColor("#334455")),
             NamedColor.Create("TestColor211",
                UIHelper.ParseColor("#6688aa")),
        };

        private static List<NamedColor> _extraColors2 = new List<NamedColor>() {
             NamedColor.Create("TestColor133",
                UIHelper.ParseColor("#123456")),
             NamedColor.Create("TestColor244",
                UIHelper.ParseColor("#80cccccc")),
        };

        public static ICollection<FontExtraInfo> SystemFonts => FontUtilities.AllFonts;
        public static FontExtraInfo RandomFont => SystemFonts.Skip(new Random().Next(20)).FirstOrDefault();


        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;
            Debug.WriteLine($"MainWindow init {RandomFont}");
            //colorBox1.ItemSource = new ObservableCollection<NamedColor>(ColorComboBox.AllSystemColors);
        }

        private void ColorBox_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<NamedColor> e) {
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

        public static IEnumerable<string> SampleValues => Enumerable.Range(50, 20).Select(it => $"简单数据项 Simple Item No.{it}");


        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine("Window_Loaded");
            //Task.Run(async () => {
            //    await Task.Delay(5000);
            //    Dispatcher.Invoke(() => {
            //        Debug.WriteLine("Window_Loaded change color");
            //        if (colorBox1.Items[0] is NamedColor color) {
            //            var preIndex = colorBox1.SelectedIndex;
            //            //color.Key = "Changed";
            //            //color.Value = Colors.Yellow;
            //            var newColor = NamedColor.Create("Changed", Colors.Yellow);
            //            //colorBox1.ItemSource.RemoveAt(0);
            //            //colorBox1.ItemSource.Insert(0, newColor);
            //            colorBox1.ItemSource[0] = newColor;
            //            // important, update index, or index = -1;
            //            colorBox1.SelectedIndex = preIndex;

            //            Debug.WriteLine($"Window_Loaded change {colorBox1.SelectedIndex} {colorBox1.SelectedItem}");
            //        }
            //    });
            //});
        }

        private void SimpleCombo_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Debug.WriteLine($"SimpleCombo_SelectionChanged old={e.OldValue} new={e.NewValue}");
        }
    }
}
