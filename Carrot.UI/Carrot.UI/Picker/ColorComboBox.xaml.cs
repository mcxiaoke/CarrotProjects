using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Carrot.UI.Controls;
using Carrot.UI.Controls.Utils;
using Color = System.Windows.Media.Color;

namespace Carrot.UI.Controls.Picker {

    public class ColorComboBoxItem {

        public static readonly ColorComboBoxItem INVALID = ColorComboBoxItem.Create("Invalid", UIHelper.ParseColor("#000001"));

        public static ColorComboBoxItem Create(string key, Color value) => new ColorComboBoxItem(key, value);
        public static ColorComboBoxItem Create(string key, string hex) => new ColorComboBoxItem(key, UIHelper.ParseColor(hex));

        public string Key { get; set; }
        public Color Value { get; set; }

        internal ColorComboBoxItem(string key, Color value) {
            Key = key ?? Convert.ToString(value);
            Value = value;
        }

        public override string ToString() {
            return $"{Key} {Value}";
        }
    }
    /// <summary>
    /// ColorComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class ColorComboBox : UserControl {

        #region DependencyProperty

        public static readonly DependencyProperty ExtraColorsProperty =
    DependencyProperty.Register(nameof(ExtraColors), typeof(List<ColorComboBoxItem>),
        typeof(ColorComboBox), new PropertyMetadata(null));

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(nameof(ColumnCount), typeof(int),
                typeof(ColorComboBox), new PropertyMetadata(1));

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int),
                typeof(ComboBox), new PropertyMetadata(-1));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object),
                typeof(ComboBox), new PropertyMetadata(null));

        #endregion

        public List<ColorComboBoxItem> ExtraColors {
            get => (List<ColorComboBoxItem>)GetValue(ExtraColorsProperty);
            set { SetValue(ExtraColorsProperty, value); }
        }

        public int ColumnCount {
            get => Utilities.Clamp((int)GetValue(ColumnCountProperty), 1, 6);
            set => SetValue(ColumnCountProperty, Utilities.Clamp(value, 1, 6));
        }

        public int SelectedIndex {
            get => cmbColors.SelectedIndex;
            set => cmbColors.SelectedIndex = value;
        }

        public object SelectedItem {
            get => cmbColors.SelectedItem;
            set => cmbColors.SelectedItem = value;
        }

        public ColorComboBox() {
            InitializeComponent();
            Debug.WriteLine("ColorComboBox_Init");
        }


        private static IEnumerable<ColorComboBoxItem> GetAllColors() =>
            typeof(Colors).GetProperties()
            .Where(prop => typeof(Color).IsAssignableFrom(prop.PropertyType))
            .Select(prop => ColorComboBoxItem.Create(prop.Name, (Color)prop.GetValue(null)));

        private void ColorComboBox_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine($"ColorComboBox_Loaded {Name}");
            //ExtraColors?.ForEach(item => Debug.WriteLine($"ColorComboBox_Loaded extra {item.Key} {item.Value}"));
            var allColors = new List<ColorComboBoxItem>();
            if (ExtraColors?.Count > 0) {
                allColors.AddRange(ExtraColors);
            }
            var sysColors = GetAllColors().ToList();
            allColors.AddRange(sysColors);
            cmbColors.ItemsSource = allColors;
        }

        private void ComboBox_Table_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine($"ComboBox_Table_Loaded {Name} {cmbColors.Items.Count}");
            if (sender is Grid grid) {
                if (grid.RowDefinitions.Count == 0) {
                    for (int r = 0; r <= cmbColors.Items.Count / ColumnCount; r++) {
                        grid.RowDefinitions.Add(new RowDefinition());
                    }
                }
                if (grid.ColumnDefinitions.Count == 0) {
                    for (int c = 0; c < Math.Min(cmbColors.Items.Count, ColumnCount); c++) {
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                }
                for (int i = 0; i < grid.Children.Count; i++) {
                    Grid.SetColumn(grid.Children[i], i % ColumnCount);
                    Grid.SetRow(grid.Children[i], i / ColumnCount);
                }
            }
        }

        #region Event Handlers
        private void CmbColors_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var oldColor = ColorComboBoxItem.INVALID;
            var newColor = ColorComboBoxItem.INVALID;
            if (e.RemovedItems.Count > 0) {
                oldColor = (ColorComboBoxItem)e.RemovedItems[0];
            }
            if (e.AddedItems.Count > 0) {
                newColor = (ColorComboBoxItem)e.AddedItems[0];
            }
            Debug.WriteLine($"SelectionChanged {Name} old={oldColor} new={newColor}");
            var newEventArgs = new RoutedPropertyChangedEventArgs<ColorComboBoxItem>
                (oldColor, newColor) { RoutedEvent = SelectedColorChangedEvent };

            RaiseEvent(newEventArgs);
        }



        public static readonly RoutedEvent SelectedColorChangedEvent =
    EventManager.RegisterRoutedEvent(nameof(SelectedColorChanged),
        RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<ColorComboBoxItem>),
        typeof(ColorComboBox));


        public event RoutedPropertyChangedEventHandler<ColorComboBoxItem> SelectedColorChanged {
            add {
                AddHandler(SelectedColorChangedEvent, value);
            }

            remove {
                RemoveHandler(SelectedColorChangedEvent, value);
            }
        }


        private void CmbColors_DropDownClosed(object sender, EventArgs e) {
            Debug.WriteLine($"CmbColors_DropDownClosed {Name} selected={cmbColors.SelectedItem}");
        }

        #endregion
    }


}
