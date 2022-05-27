using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

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
            this.OptionEnableNotifications.Checked = settings.OptionEnableNotifications;
            this.OptionAutoUpdate.Checked = settings.OptionAutoUpdate;
        }

        private void OptionForm_FormClosing(object sender, FormClosingEventArgs e) {
            var settings = Properties.Settings.Default;
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
            if (settings.OptionEnableNotifications != this.OptionEnableNotifications.Checked) { settings.OptionEnableNotifications = this.OptionEnableNotifications.Checked; }
            if (settings.OptionAutoUpdate != this.OptionAutoUpdate.Checked) { settings.OptionAutoUpdate = this.OptionAutoUpdate.Checked; }
            settings.Save();
        }

        private void ProjectLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            Logger.Debug($"ProjectLabel_LinkClicked {e.Link}");
            ProjectLabel.LinkVisited = true;
            if (ProjectLabel.Text.Contains("发现新版本")) {
                if (ShowUpdater()) {
                    return;
                }
            }
            System.Diagnostics.Process.Start(AutoUpdater.ProjectUrl);
        }

        private void CloseButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void CheckButton_Click(object sender, EventArgs e) {
            if (!ShowUpdater()) {
                var dialogResult = MessageBox.Show($"自动更新程序 SharpUpdater.exe 丢失，" +
                    $"建议手动更新，是否打开浏览器访问项目页面 {AutoUpdater.ProjectUrl} 下载最新版本？",
                    "无法启动更新程序", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes) {
                    Process.Start(AutoUpdater.ProjectUrl);
                }
            }
        }

        private bool ShowUpdater() {
            var updater = Path.Combine(Application.StartupPath, "SharpUpdater.exe");
            var name = Application.ProductName;
            var url = AutoUpdater.VersionUrls[0];
            if (File.Exists(updater)) {
                ProcessStartInfo startInfo = new ProcessStartInfo(updater) {
                    WindowStyle = ProcessWindowStyle.Normal,
                    Arguments = $"--name {name} --url \"{url}\""
                };
                Process.Start(startInfo);
                return true;
            }
            return false;
        }

        private void OptionForm_Shown(object sender, EventArgs e) {
            var info = AutoUpdater.CachedVersionInfo;
            if (AutoUpdater.HasNewVersion && info != null) {
                ProjectLabel.Text = $"发现新版本 {info.Version}";
            }
        }
    }
}
