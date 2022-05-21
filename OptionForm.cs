using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenshinNotifier {
    public partial class OptionForm : Form {
        public OptionForm() {
            InitializeComponent();
        }

        private void OptionForm_Load(object sender, EventArgs e) {
            var settings = Properties.Settings.Default;
            this.OptionAutoStart.Checked = settings.OptionAutoStart;
            this.OptionHideToTray.Checked = settings.OptionHideToTray;
            this.OptionCloseConfirm.Checked = settings.OptionCloseConfirm;
            this.OptionRefreshOnStart.Checked = settings.OptionRefreshOnStart;
            this.OptionCheckinOnStart.Checked = settings.OptionCheckinOnStart;
            this.OptionRemindResin.Checked = settings.OptionRemindResin;
            this.OptionRemindCoin.Checked = settings.OptionRemindCoin;
            this.OptionRemindTask.Checked = settings.OptionRemindTask;
            this.OptionRemindDiscount.Checked = settings.OptionRemindDiscount;
            this.OptionRemindExpedition.Checked = settings.OptionRemindExpedition;
            this.OptionRemindTransformer.Checked = settings.OptionRemindTransformer;
        }

        private void OptionForm_FormClosing(object sender, FormClosingEventArgs e) {
            var settings = Properties.Settings.Default;
            //settings.OptionAutoStart = this.OptionAutoStart.Checked;
            //settings.OptionHideToTray = this.OptionHideToTray.Checked;
            //settings.OptionCloseToTray = this.OptionCloseToTray.Checked;
            //settings.OptionRefreshOnStart = this.OptionRefreshOnStart.Checked;
            //settings.OptionCheckinOnStart = this.OptionCheckinOnStart.Checked;
            //settings.OptionRemindResin = this.OptionRemindResin.Checked;
            //settings.OptionRemindCoin = this.OptionRemindCoin.Checked;
            //settings.OptionRemindTask = this.OptionRemindTask.Checked;
            //settings.OptionRemindDiscount = this.OptionRemindDiscount.Checked;
            //settings.OptionRemindExpedition = this.OptionRemindExpedition.Checked;
            //settings.OptionRemindTransformer = this.OptionRemindTransformer.Checked;
            if (settings.OptionAutoStart != this.OptionAutoStart.Checked) { settings.OptionAutoStart = this.OptionAutoStart.Checked; }
            if (settings.OptionHideToTray != this.OptionHideToTray.Checked) { settings.OptionHideToTray = this.OptionHideToTray.Checked; }
            if (settings.OptionCloseConfirm != this.OptionCloseConfirm.Checked) { settings.OptionCloseConfirm = this.OptionCloseConfirm.Checked; }
            if (settings.OptionRefreshOnStart != this.OptionRefreshOnStart.Checked) { settings.OptionRefreshOnStart = this.OptionRefreshOnStart.Checked; }
            if (settings.OptionCheckinOnStart != this.OptionCheckinOnStart.Checked) { settings.OptionCheckinOnStart = this.OptionCheckinOnStart.Checked; }
            if (settings.OptionRemindResin != this.OptionRemindResin.Checked) { settings.OptionRemindResin = this.OptionRemindResin.Checked; }
            if (settings.OptionRemindCoin != this.OptionRemindCoin.Checked) { settings.OptionRemindCoin = this.OptionRemindCoin.Checked; }
            if (settings.OptionRemindTask != this.OptionRemindTask.Checked) { settings.OptionRemindTask = this.OptionRemindTask.Checked; }
            if (settings.OptionRemindDiscount != this.OptionRemindDiscount.Checked) { settings.OptionRemindDiscount = this.OptionRemindDiscount.Checked; }
            if (settings.OptionRemindExpedition != this.OptionRemindExpedition.Checked) { settings.OptionRemindExpedition = this.OptionRemindExpedition.Checked; }
            if (settings.OptionRemindTransformer != this.OptionRemindTransformer.Checked) { settings.OptionRemindTransformer = this.OptionRemindTransformer.Checked; }
            settings.Save();
        }

        private void ProjectLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Logger.Debug($"ProjectLabel_LinkClicked {e.Link}");
            ProjectLabel.LinkVisited = true;
            System.Diagnostics.Process.Start("https://gitee.com/osap/GenshinNotifier");
        }

        private void CloseButton_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
