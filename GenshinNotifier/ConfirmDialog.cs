using System;
using System.Windows.Forms;

namespace GenshinNotifier {

    public partial class ConfirmDialog : Form {
        private string TitleText;
        private string TextA;
        private string TextB;

        public ConfirmDialog(string title, string textA, string textB) {
            this.TitleText = title;
            this.TextA = textA;
            this.TextB = textB;
            InitializeComponent();
        }

        private void ConfirmDialog_Load(object sender, EventArgs e) {
            this.Text = TitleText;
            this.ButtonNo.Text = TextA;
            this.ButtonYes.Text = TextB;
        }

        private void ButtonA_Click(object sender, EventArgs e) {
            Close();
        }

        private void ButtonB_Click(object sender, EventArgs e) {
            Close();
        }
    }
}