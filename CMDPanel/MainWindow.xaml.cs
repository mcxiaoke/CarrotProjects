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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace CMDPanel {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private ObservableCollection<TabModel> AllTabs = new ObservableCollection<TabModel>();


        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            for (int i = 0; i < 4; i++) {
                var header = new TabHeader($"Tab Header {i}");
                var model = new TabModel(header);
                for (int j = 0; j < 1000 - i * 30; j++) {
                    model.Content.Add(new TabContent($"Tab {i} Item No. {j}", 0));
                }
                //Debug.WriteLine(model.Content.Count);
                AllTabs.Add(model);
            }
            this.DataContext = AllTabs;
        }
    }
}
