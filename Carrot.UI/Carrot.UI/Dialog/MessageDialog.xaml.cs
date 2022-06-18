using System;
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
using System.Windows.Shapes;

namespace Carrot.UI.Controls.Dialog {

    public partial class MessageDialog : Window {
        public MessageDialog() {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        public static bool Show(Window owner, string message,
            string title = "提示",
            string okText = "确定",
            string cancelText = "取消") {
            var dialog = new MessageDialog();
            dialog.Topmost = true;
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.textMessage.Text = message;
            dialog.Title = title;
            dialog.btnOk.Content = okText;
            dialog.btnCancel.Content = cancelText;
            return dialog.ShowDialog() ?? false;
        }

    }
}
