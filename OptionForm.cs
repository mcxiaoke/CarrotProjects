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
using GenshinNotifier.Properties;
using System.Configuration;

namespace GenshinNotifier {
    public partial class OptionForm : Form {
        public OptionForm() {
            InitializeComponent();
        }

        private void OptionForm_Load(object sender, EventArgs e) {
        }

        private void OptionForm_FormClosing(object sender, FormClosingEventArgs e) {
            // why property binding not auto saved?
            Settings.Default.Save();
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

        private void OptionForm_FormClosed(object sender, FormClosedEventArgs e) {
        }
    }
}
