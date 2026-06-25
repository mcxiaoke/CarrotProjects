using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MuteMe {
    public partial class AutoMute : Form {
        private readonly AppConfig _config;
        private readonly AudioController _audioController;
        private readonly System.Windows.Forms.Timer _monitorTimer;
        private readonly Dictionary<string, DateTime> _muteTimers;
        private bool _isExiting;
        private string? _lastForegroundProcess;

        public AutoMute() {
            InitializeComponent();
            LoadTrayIcon();
            _config = ConfigManager.Load();
            _audioController = new AudioController();
            _muteTimers = new Dictionary<string, DateTime>();
            _monitorTimer = new System.Windows.Forms.Timer {
                Interval = 500
            };
            _monitorTimer.Tick += MonitorTimer_Tick;
        }

        private void LoadTrayIcon() {
            try {
                var exePath = AppDomain.CurrentDomain.BaseDirectory;
                var iconPath = Path.Combine(exePath, "app.ico");
                if (File.Exists(iconPath)) {
                    var icon = new Icon(iconPath);
                    notifyIcon.Icon = icon;
                    Icon = icon;
                    return;
                }
            } catch {
            }
            notifyIcon.Icon = SystemIcons.Application;
            Icon = SystemIcons.Application;
        }

        private void AutoMute_Load(object? sender, EventArgs e) {
            foreach (var name in _config.ProcessNames) {
                lstProcesses.Items.Add(name);
            }
            numDelay.Value = _config.DelaySeconds;
            _monitorTimer.Start();
            UpdateStatus("运行中");
        }

        private void TxtProcessName_KeyPress(object? sender, KeyPressEventArgs e) {
            if (e.KeyChar == (char)Keys.Enter) {
                AddProcess();
                e.Handled = true;
            }
        }

        private void BtnAdd_Click(object? sender, EventArgs e) {
            AddProcess();
        }

        private void AddProcess() {
            var name = txtProcessName.Text.Trim();
            if (string.IsNullOrEmpty(name)) return;

            name = NativeMethods.NormalizeProcessName(name);

            if (lstProcesses.Items.Contains(name)) {
                MessageBox.Show("该进程已在列表中", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            lstProcesses.Items.Add(name);
            _config.ProcessNames.Add(name);
            ConfigManager.Save(_config);
            txtProcessName.Clear();
        }

        private void BtnRemove_Click(object? sender, EventArgs e) {
            if (lstProcesses.SelectedItem == null) {
                MessageBox.Show("请先选择要删除的进程", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var name = lstProcesses.SelectedItem.ToString();
            if (string.IsNullOrEmpty(name)) return;
            lstProcesses.Items.Remove(lstProcesses.SelectedItem);
            _config.ProcessNames.Remove(name);
            ConfigManager.Save(_config);
        }

        private void NumDelay_ValueChanged(object? sender, EventArgs e) {
            _config.DelaySeconds = (int)numDelay.Value;
            ConfigManager.Save(_config);
        }

        private void BtnMinimize_Click(object? sender, EventArgs e) {
            HideToTray();
        }

        private void AutoMute_FormClosing(object? sender, FormClosingEventArgs e) {
            if (!_isExiting) {
                e.Cancel = true;
                HideToTray();
            } else {
                _monitorTimer.Stop();
                _audioController.Dispose();
                notifyIcon.Visible = false;
            }
        }

        private void HideToTray() {
            Hide();
            notifyIcon.Visible = true;
        }

        private void NotifyIcon_DoubleClick(object? sender, EventArgs e) {
            ShowWindow();
        }

        private void MenuShow_Click(object? sender, EventArgs e) {
            ShowWindow();
        }

        private void MenuExit_Click(object? sender, EventArgs e) {
            _isExiting = true;
            Application.Exit();
        }

        private void ShowWindow() {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void MonitorTimer_Tick(object? sender, EventArgs e) {
            var foregroundProcess = NativeMethods.GetForegroundProcessName();
            if (foregroundProcess == null) return;

            if (foregroundProcess == _lastForegroundProcess) return;
            _lastForegroundProcess = foregroundProcess;

            foreach (var item in lstProcesses.Items) {
                var processName = item.ToString();
                if (string.IsNullOrEmpty(processName)) continue;

                var isForeground = string.Equals(processName, foregroundProcess, StringComparison.OrdinalIgnoreCase);

                if (isForeground) {
                    if (_muteTimers.ContainsKey(processName)) {
                        _muteTimers.Remove(processName);
                    }
                    _audioController.SetMuteForProcess(processName, false);
                } else {
                    if (!_muteTimers.ContainsKey(processName)) {
                        _muteTimers[processName] = DateTime.Now;
                    } else {
                        var elapsed = (DateTime.Now - _muteTimers[processName]).TotalSeconds;
                        if (elapsed >= _config.DelaySeconds) {
                            _audioController.SetMuteForProcess(processName, true);
                        }
                    }
                }
            }
        }

        private void UpdateStatus(string status) {
            lblStatus.Text = $"状态: {status}";
            lblStatus.ForeColor = status == "运行中" ? Color.Green : Color.Gray;
        }
    }
}
