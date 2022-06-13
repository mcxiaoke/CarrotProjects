using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NotifierWidget {

    /// <summary>
    /// OptionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OptionWindow : Window {
        // https://stackoverflow.com/questions/30440634
        // https://wpf-tutorial.com/list-controls/combobox-control/

        private IEnumerable<KeyValuePair<string, Color>> GetAllColors() =>
            typeof(Colors).GetProperties()
        .Where(prop => typeof(Color).IsAssignableFrom(prop.PropertyType))
        .Select(prop => new KeyValuePair<string, Color>(prop.Name, (Color)prop.GetValue(null)));

        public OptionWindow() {
            InitializeComponent();
            var colors = GetAllColors().ToList();
            var dc = new KeyValuePair<string, Color>("App", Colors.Red);
            colors.Insert(0, dc);

            foreach (KeyValuePair<string, Color> item in colors) {
                Debug.WriteLine($"{item.Key} {item.Value}");
            }
            cmbColors.ItemsSource = colors;
            cmbColors.SelectedIndex = 0;
        }

        private static readonly int COLUMN_COUNT = 3;

        private void ComboBox_Table_Loaded(object sender, RoutedEventArgs e) {
            if (sender is Grid grid) {
                if (grid.RowDefinitions.Count == 0) {
                    for (int r = 0; r <= cmbColors.Items.Count / COLUMN_COUNT; r++) {
                        grid.RowDefinitions.Add(new RowDefinition());
                    }
                }
                if (grid.ColumnDefinitions.Count == 0) {
                    for (int c = 0; c < Math.Min(cmbColors.Items.Count, COLUMN_COUNT); c++) {
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                    }
                }
                for (int i = 0; i < grid.Children.Count; i++) {
                    Grid.SetColumn(grid.Children[i], i % COLUMN_COUNT);
                    Grid.SetRow(grid.Children[i], i / COLUMN_COUNT);
                }
            }
        }
    }
}