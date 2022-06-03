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
using CarrotCommon;

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
                AutoUpdater.ShowUpdater();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e) {
            Close();
        }

        private void CheckButton_Click(object sender, EventArgs e) {
            AutoUpdater.ShowUpdater();
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
