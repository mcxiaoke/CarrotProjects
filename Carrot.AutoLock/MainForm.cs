using Carrot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Carrot.AutoLock;

/// <summary>
/// Main application form. Controls tray icon and menu logic.
/// 主窗体类。控制系统托盘图标和菜单逻辑。
/// </summary>
public partial class MainForm : Form {

    private const string NAME = "CarrotLock";
    private const int NotifyTextMaxLength = 63;

    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenuStrip;
    private readonly AppConfig _appConfig = new();
    private readonly ActiveChecker _checker;

    public MainForm() {
        Logger.Info(@"MainForm()");
        _checker = new ActiveChecker();
        _notifyIcon = new NotifyIcon {
            Icon = Properties.Resources.carrot_512,
            Text = NAME
        };

        _contextMenuStrip = new ContextMenuStrip();

        var showMenuItem = new ToolStripMenuItem("显示窗口", null, ShowWindowMenuItem_Click);
        _contextMenuStrip.Items.Add(showMenuItem);

        _contextMenuStrip.Items.Add(new ToolStripSeparator());

        var exitMenuItem = new ToolStripMenuItem("退出应用", null, ExitMenuItem_Click);
        _contextMenuStrip.Items.Add(exitMenuItem);

        _notifyIcon.ContextMenuStrip = _contextMenuStrip;
        _notifyIcon.Click += NotifyIcon_Click;

        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e) {
        Logger.Debug(@"MainForm_Load");

        _appConfig.Load();

        cbAutoStart.Checked = _appConfig.Data.AutoStartEnabled;

        UpdateConfigDisplay();
        UpdateUI();
        ToggleCheck();
    }

    private void MainForm_Resize(object sender, EventArgs e) {
        Logger.Debug($@"MainForm_Resize {this.WindowState}");
        if (this.WindowState == FormWindowState.Minimized) {
            this.Hide();
            _notifyIcon.Visible = true;
        } else {
            ShowWindow();
        }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
        Logger.Debug($@"MainForm_FormClosing Reason:{e.CloseReason}");
        if (e.CloseReason == CloseReason.UserClosing) {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            _notifyIcon.Visible = true;
            return;
        }

        _appConfig.Save();
        _checker.Stop();
        _checker.Callback = null;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenuStrip.Dispose();
    }

    private void NotifyIcon_Click(object? sender, EventArgs e) {
        if (e is MouseEventArgs { Button: MouseButtons.Left }) {
            ShowWindow();
        } else if (e is MouseEventArgs { Button: MouseButtons.Right }) {
            _contextMenuStrip.Show(Cursor.Position);
        }
    }

    private void ShowWindowMenuItem_Click(object? sender, EventArgs e) {
        ShowWindow();
    }

    private void ExitMenuItem_Click(object? sender, EventArgs e) {
        Logger.Debug(@"ExitMenuItem_Click");
        _checker.Stop();
        Application.Exit();
    }

    private void BtnExit_Click(object sender, EventArgs e) {
        Logger.Debug(@"BtnExit_Click");
        _appConfig.Save();
        _checker.Stop();
        _checker.Callback = null;
        Application.Exit();
    }

    private void BtnStart_Click(object sender, EventArgs e) {
        Logger.Debug(@"BtnStart_Click");
        ToggleCheck();
    }

    private LogViewerForm? _logViewerForm;

    private void BtnViewLog_Click(object sender, EventArgs e) {
        if (_logViewerForm == null || _logViewerForm.IsDisposed) {
            _logViewerForm = new LogViewerForm();
            _logViewerForm.Show(this);
        } else {
            _logViewerForm.Activate();
        }
    }

    private void BtnSettings_Click(object sender, EventArgs e) {
        if (_checker.IsRunning()) {
            MessageBox.Show(@"请先停止运行后再打开设置", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var settingsForm = new SettingsForm(_appConfig);
        if (settingsForm.ShowDialog(this) == DialogResult.OK) {
            UpdateConfigDisplay();
        }
    }

    private void ToggleCheck() {
        if (_checker.IsRunning()) {
            _checker.Stop();
            _checker.Callback = null;
        } else {
            _checker.SetTargetIP(_appConfig.Data.TargetIP);
            _checker.SetTargetBluetoothName(_appConfig.Data.TargetBluetoothName);
            _checker.SetRouterPassword(_appConfig.Data.RouterPassword);
            _checker.SetTimeoutSecs(_appConfig.Data.OfflineTimeoutSeconds, _appConfig.Data.InactiveTimeoutSeconds);
            _checker.SetExemptProcesses(_appConfig.Data.ExemptProcesses);
            _checker.SetWebSocketUri(_appConfig.Data.WebSocketUri);

            ConfigureNotifiers();

            _checker.Callback = OnStatusChanged;
            _checker.Start();
        }
        UpdateUI();
    }

    private void ConfigureNotifiers() {
        var notificationManager = _checker.GetNotificationManager();
        notificationManager.ClearNotifiers();

        if (!string.IsNullOrWhiteSpace(_appConfig.Data.WeChatWebhookKey)) {
            var weChatNotifier = new WeChatNotifier(_appConfig.Data.WeChatWebhookKey);
            notificationManager.AddNotifier(weChatNotifier);
        }

        if (!string.IsNullOrWhiteSpace(_appConfig.Data.TelegramBotToken) &&
            !string.IsNullOrWhiteSpace(_appConfig.Data.TelegramChatId)) {
            var telegramNotifier = new TelegramNotifier(_appConfig.Data.TelegramBotToken, _appConfig.Data.TelegramChatId, "http://127.0.0.1:7890");
            notificationManager.AddNotifier(telegramNotifier);
        }
    }

    public void OnStatusChanged(string result) {
        if (InvokeRequired) {
            Invoke(new MethodInvoker(() => UpdateUI(result)));
        } else {
            UpdateUI(result);
        }
    }

    private void UpdateConfigDisplay() {
        var lines = new List<string> {
            $"目标IP: {_appConfig.Data.TargetIP} | 蓝牙名称: {_appConfig.Data.TargetBluetoothName}",
            $"离线超时: {_appConfig.Data.OfflineTimeoutSeconds}s | 空闲超时: {_appConfig.Data.InactiveTimeoutSeconds}s",
            $"路由器密码: {(!string.IsNullOrEmpty(_appConfig.Data.RouterPassword) ? "已配置" : "未配置")}",
            $"企业微信: {(!string.IsNullOrEmpty(_appConfig.Data.WeChatWebhookKey) ? "已配置" : "未配置")} | Telegram: {(!string.IsNullOrEmpty(_appConfig.Data.TelegramBotToken) ? "已配置" : "未配置")}",
            $"WebSocket: {(!string.IsNullOrEmpty(_appConfig.Data.WebSocketUri) ? _appConfig.Data.WebSocketUri : "未配置")}",
            $"豁免进程: {(_appConfig.Data.ExemptProcesses.Count > 0 ? string.Join(", ", _appConfig.Data.ExemptProcesses) : "无")}"
        };

        ConfigText.Text = string.Join("\r\n", lines);
    }

    private void UpdateUI(string? statusInfo = null) {
        if (_checker == null || _notifyIcon == null) return;

        var running = _checker.IsRunning();

        btnStart.Text = running ? "停止" : "启动";
        btnSettings.Enabled = !running;

        var textLines = new List<string> {
            $"状态: {(running ? "运行中" : "已停止")} | 设备: {(_checker.IsDeviceOnline() ? "在线" : "离线")}",
            $"离线计时: {_checker.OfflineSeconds:F0}s / {_appConfig.Data.OfflineTimeoutSeconds}s",
            $"空闲计时: {ActiveChecker.GetInactiveSeconds():F0}s / {_appConfig.Data.InactiveTimeoutSeconds}s",
            $"通知器: {_checker.GetNotificationManager().ConfiguredCount} 个"
        };

        InfoText.Text = string.Join("\r\n", textLines);

        _notifyIcon.Text = BuildNotifyText(running);
    }

    private void CbAutoStart_CheckedChanged(object? sender, EventArgs e) {
        _appConfig.Data.AutoStartEnabled = cbAutoStart.Checked;
        SetAutoStart(cbAutoStart.Checked, Application.ProductName ?? NAME, Application.ExecutablePath ?? string.Empty);
    }

    private static void SetAutoStart(bool enable, string appName, string appPath) {
        Logger.Info($@"SetAutoStart Enable:{enable} AppPath:{appPath}");
        try {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null) return;

            if (enable) {
                var quotedPath = string.IsNullOrWhiteSpace(appPath) ? string.Empty : $"\"{appPath}\"";
                key.SetValue(appName, quotedPath);
            } else {
                key.DeleteValue(appName, false);
            }
        } catch (Exception ex) {
            Logger.Error("SetAutoStart", ex);
            MessageBox.Show($@"Failed to set auto-start: {ex.Message}", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void ShowWindow() {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
        _notifyIcon.Visible = false;
    }

    private string BuildNotifyText(bool running) {
        var deviceInfo = string.IsNullOrEmpty(_appConfig.Data.TargetBluetoothName)
            ? _appConfig.Data.TargetIP
            : $"{_appConfig.Data.TargetIP}/{_appConfig.Data.TargetBluetoothName}";

        var text = running ? $"{NAME} - Running {deviceInfo}" : $"{NAME} - Stopped";
        text = text.Replace('\r', ' ').Replace('\n', ' ');

        if (text.Length > NotifyTextMaxLength) {
            text = text[..NotifyTextMaxLength];
        }

        return text;
    }

    private void ConfigText_TextChanged(object sender, EventArgs e) {

    }
}
