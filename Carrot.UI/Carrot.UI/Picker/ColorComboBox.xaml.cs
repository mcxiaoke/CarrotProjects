using System;
using System.Diagnostics;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Carrot.UI.Controls;
using Carrot.UI.Controls.Utils;
using Color = System.Windows.Media.Color;
using System.Collections;

namespace Carrot.UI.Controls.Picker {


    /// <summary>
    /// ColorComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class ColorComboBox : UserControl {


        public static IEnumerable<NamedColor> AllSystemColors =>
            typeof(Colors).GetProperties()
            .Where(prop => typeof(Color).IsAssignableFrom(prop.PropertyType))
            .Select(prop => NamedColor.Create(prop.Name, (Color)prop.GetValue(null)!));

        #region DependencyProperty

        public static readonly DependencyProperty ExtraColorsProperty =
    DependencyProperty.Register(nameof(ExtraColors), typeof(List<NamedColor>),
        typeof(ColorComboBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ItemSourceProperty =
DependencyProperty.Register(nameof(ItemSource), typeof(ObservableCollection<NamedColor>),
typeof(ColorComboBox), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(nameof(ColumnCount), typeof(int),
                typeof(ColorComboBox), new UIPropertyMetadata(1));

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int),
                typeof(ComboBox), new UIPropertyMetadata(-1));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(NamedColor),
                typeof(ComboBox), new UIPropertyMetadata(null));

        #endregion

        public ICollection<NamedColor> ExtraColors {
            get => (ICollection<NamedColor>)GetValue(ExtraColorsProperty);
            set { SetValue(ExtraColorsProperty, value); }
        }

        public ObservableCollection<NamedColor> ItemSource {
            get => (ObservableCollection<NamedColor>)GetValue(ItemSourceProperty);
            set { SetValue(ItemSourceProperty, value); }
        }

        public int ColumnCount {
            get => Utilities.Clamp((int)GetValue(ColumnCountProperty), 1, 6);
            set => SetValue(ColumnCountProperty, Utilities.Clamp(value, 1, 6));
        }

        public int SelectedIndex {
            get => cmbColors.SelectedIndex;
            set => cmbColors.SelectedIndex = value;
        }

        public NamedColor SelectedItem {
            get => (NamedColor)cmbColors.SelectedItem;
            set => cmbColors.SelectedItem = value;
        }

        public ItemCollection Items => cmbColors.Items;

        public ColorComboBox() {
            InitializeComponent();
            Debug.WriteLine("ColorComboBox_Init");
            cmbColors.DataContext = this;
            var allColors = new List<NamedColor>();
            if (ExtraColors?.Count > 0) {
                allColors.AddRange(ExtraColors);
            }
            allColors.AddRange(AllSystemColors);
            ItemSource = new ObservableCollection<NamedColor>(allColors);
            //cmbColors.ItemsSource = ItemSource;
        }


        private void ColorComboBox_Loaded(object sender, RoutedEventArgs e) {
            Debug.WriteLine($"ColorComboBox_Loaded {Name}");

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
            if (e.RemovedItems.Count > 0 && e.RemovedItems[0] is NamedColor removedItem) {
                oldItem = removedItem;
                Debug.WriteLine($"CmbColors_SelectionChanged removed={removedItem}");
            }
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is NamedColor addedItem) {
                Debug.WriteLine($"CmbColors_SelectionChanged added={addedItem}");
            }
        }

        public static readonly RoutedEvent SelectedColorChangedEvent =
    EventManager.RegisterRoutedEvent(nameof(SelectedColorChanged),
        RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<NamedColor>),
        typeof(ColorComboBox));


        public event RoutedPropertyChangedEventHandler<NamedColor> SelectedColorChanged {
            add {
                AddHandler(SelectedColorChangedEvent, value);
            }

            remove {
                RemoveHandler(SelectedColorChangedEvent, value);
            }
        }


        private NamedColor oldItem = NamedColor.INVALID;
        private void CmbColors_DropDownClosed(object sender, EventArgs e) {
            var newItem = this.SelectedItem;
            var newEventArgs = new RoutedPropertyChangedEventArgs<NamedColor>
    (oldItem, newItem) { RoutedEvent = SelectedColorChangedEvent };
            Debug.WriteLine($"CmbColors_DropDownClosed {Name} index={this.SelectedIndex} old={oldItem} new={newItem}");
            RaiseEvent(newEventArgs);
        }

        #endregion
    }


}
