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
using System.Windows.Shapes;

namespace Carrot.UI.Samples {
    /// <summary>
    /// Demo.xaml 的交互逻辑
    /// </summary>
    public partial class Demo : Window {
        public Demo() {
            InitializeComponent();
            simpleCombo.DataContext = this;
        }

        public static IEnumerable<string> SampleValues => Enumerable.Range(50, 20)
            .Select(it => $"简单数据项 Simple No.{it}");
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            simpleCombo.ItemSource = SampleValues;

        }

        private void SimpleCombo_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Debug.WriteLine($"SimpleCombo_SelectionChanged old={e.OldValue} new={e.NewValue}");
        }
    }
}
