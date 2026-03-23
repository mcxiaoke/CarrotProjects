using Carrot.Common;
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
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

    /// <summary>
    /// Initializes the main form and tray icon.
    /// 初始化主窗体和托盘图标。
    /// </summary>
    public MainForm() {
        Logger.Info(@"MainForm()");
        _checker = new ActiveChecker();
        // Initialize NotifyIcon
        _notifyIcon = new NotifyIcon {
            Icon = Properties.Resources.carrot_512,
            Text = NAME
        };

        // Initialize ContextMenuStrip
        _contextMenuStrip = new ContextMenuStrip();

        // Show Window menu item
        var showMenuItem = new ToolStripMenuItem("显示窗口", null, ShowWindowMenuItem_Click);
        _contextMenuStrip.Items.Add(showMenuItem);

        // Add separator
        _contextMenuStrip.Items.Add(new ToolStripSeparator());

        // Exit menu item
        var exitMenuItem = new ToolStripMenuItem("退出应用", null, ExitMenuItem_Click);
        _contextMenuStrip.Items.Add(exitMenuItem);

        // Bind ContextMenuStrip to NotifyIcon
        _notifyIcon.ContextMenuStrip = _contextMenuStrip;
        _notifyIcon.Click += NotifyIcon_Click;

        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e) {
        Logger.Debug(@"MainForm_Load");

        // 加载配置
        _appConfig.Load();

        // 填充 UI
        textIPAddress.Text = _appConfig.Data.TargetIP;
        textBluetoothName.Text = _appConfig.Data.TargetBluetoothName;
        textOfflineSecs.Text = _appConfig.Data.OfflineTimeoutSeconds.ToString();
        textInactiveSecs.Text = _appConfig.Data.InactiveTimeoutSeconds.ToString();
        textWeChatKey.Text = _appConfig.Data.WeChatWebhookKey;
        textTelegramToken.Text = _appConfig.Data.TelegramBotToken;
        textTelegramChatId.Text = _appConfig.Data.TelegramChatId;

        // Initialize AutoStart Checkbox
        cbAutoStart.Checked = _appConfig.Data.AutoStartEnabled;

        UpdateUI();
        ToggleCheck();
    }

    private void MainForm_Resize(object sender, EventArgs e) {
        Logger.Debug($@"MainForm_Resize {this.WindowState}");
        // Check if window is minimized
        if (this.WindowState == FormWindowState.Minimized) {
            // Hide window
            this.Hide();
            // Show tray icon
            _notifyIcon.Visible = true;
            // notifyIcon.ShowBalloonTip(1000, NAME, "Minimizing to tray...", ToolTipIcon.Info);
        } else {
            ShowWindow();
        }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
        Logger.Debug($@"MainForm_FormClosing Reason:{e.CloseReason}");
        // If user creates closing event, minimize to tray
        if (e.CloseReason == CloseReason.UserClosing) {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            _notifyIcon.Visible = true;
            return;
        }

        SaveConfiguration();
        _checker.Stop();
        _checker.Callback = null;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenuStrip.Dispose();
    }

    private void NotifyIcon_Click(object? sender, EventArgs e) {
        // Only handle left click, right click is context menu
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
        SaveConfiguration();
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
        //Logger.Debug(@"BtnViewLog_Click");
        if (_logViewerForm == null || _logViewerForm.IsDisposed) {
            _logViewerForm = new LogViewerForm();
            _logViewerForm.Show(this);
        } else {
            _logViewerForm.Activate();
        }
    }

    /// <summary>
    /// Toggles the checker status (Start/Stop).
    /// 切换检测状态 (开始/停止)。
    /// </summary>
    private void ToggleCheck() {
        if (_checker.IsRunning()) {
            _checker.Stop();
            _checker.Callback = null;
        } else {
            // 验证并保存配置
            if (!SaveConfiguration()) {
                return;
            }

            // 配置检测器
            _checker.SetTargetIP(_appConfig.Data.TargetIP);
            _checker.SetTargetBluetoothName(_appConfig.Data.TargetBluetoothName);
            _checker.SetRouterPassword(_appConfig.Data.RouterPassword);
            _checker.SetTimeoutSecs(_appConfig.Data.OfflineTimeoutSeconds, _appConfig.Data.InactiveTimeoutSeconds);

            // 配置通知器
            ConfigureNotifiers();

            _checker.Callback = OnStatusChanged;
            _checker.Start();
        }
        UpdateUI();
    }

    /// <summary>
    /// 配置通知器
    /// Configure notifiers
    /// </summary>
    private void ConfigureNotifiers() {
        var notificationManager = _checker.GetNotificationManager();
        notificationManager.ClearNotifiers();

        // 添加企业微信通知器
        if (!string.IsNullOrWhiteSpace(_appConfig.Data.WeChatWebhookKey)) {
            var weChatNotifier = new WeChatNotifier(_appConfig.Data.WeChatWebhookKey);
            notificationManager.AddNotifier(weChatNotifier);
        }

        // 添加 Telegram 通知器
        if (!string.IsNullOrWhiteSpace(_appConfig.Data.TelegramBotToken) &&
            !string.IsNullOrWhiteSpace(_appConfig.Data.TelegramChatId)) {
            var telegramNotifier = new TelegramNotifier(_appConfig.Data.TelegramBotToken, _appConfig.Data.TelegramChatId, "http://127.0.0.1:7890");
            notificationManager.AddNotifier(telegramNotifier);
        }
    }

    /// <summary>
    /// Callback for status updates, invokes UI update.
    /// 状态变更回调，触发 UI 更新。
    /// </summary>
    public void OnStatusChanged(string result) {
        //Logger.Debug("OnStatusChanged");
        if (InvokeRequired) {
            Invoke(new MethodInvoker(() => UpdateUI(result)));
        } else {
            UpdateUI(result);
        }
    }

    /// <summary>
    /// Updates the UI based on checker status.
    /// 根据检测状态更新 UI。
    /// </summary>
    private void UpdateUI(string? statusInfo = null) {
        if (_checker == null || _notifyIcon == null) return;

        var running = _checker.IsRunning();

        // 禁用/启用输入控件
        textIPAddress.Enabled = !running;
        textBluetoothName.Enabled = !running;
        textOfflineSecs.Enabled = !running;
        textInactiveSecs.Enabled = !running;
        textWeChatKey.Enabled = !running;
        textTelegramToken.Enabled = !running;
        textTelegramChatId.Enabled = !running;

        btnStart.Text = running ? "停止" : "启动";

        var textLines = new List<string> {
            $"Notifiers: {_checker.GetNotificationManager().ConfiguredCount}",
            $"Running: {running}, " + $"Online: {_checker.IsDeviceOnline()}",
            $"Offline Seconds: {_checker.OfflineSeconds:F0}s / {_appConfig.Data.OfflineTimeoutSeconds}s",
            $"Inactive Seconds: {ActiveChecker.GetInactiveSeconds():F0}s / {_appConfig.Data.InactiveTimeoutSeconds}s"
        };

        // Use InfoText (TextBox) instead of labelStatus
        InfoText.Text = string.Join("\r\n", textLines);

        _notifyIcon.Text = BuildNotifyText(running);
    }

    /// <summary>
    /// Checks if auto-start is enabled.
    /// 检查开机启动是否已启用。
    /// </summary>
    private static bool IsAutoStartEnabled(string appName) {
        try {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", false);
            return key?.GetValue(appName) != null;
        } catch {
            return false;
        }
    }

    private void TextIPAddress_TextChanged(object? sender, EventArgs e) {
        // Validate IP address format in real-time
        string ip = textIPAddress.Text.Trim();
        bool isValid = TryGetValidIpv4(ip, out _);

        // Visual feedback for invalid IP
        textIPAddress.BackColor = isValid || string.IsNullOrEmpty(ip)
            ? SystemColors.Window
            : Color.LightPink;

        // Enable/disable start button based on validity
        btnStart.Enabled = isValid || string.IsNullOrEmpty(ip);
    }

    private void CbAutoStart_CheckedChanged(object? sender, EventArgs e) {
        _appConfig.Data.AutoStartEnabled = cbAutoStart.Checked;
        SetAutoStart(cbAutoStart.Checked, Application.ProductName ?? NAME, Application.ExecutablePath ?? string.Empty);
    }

    /// <summary>
    /// Sets the auto-start registry key.
    /// 设置开机启动注册表项。
    /// </summary>
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

    /// <summary>
    /// Shows the window and hides the tray icon.
    /// 显示窗口并隐藏托盘图标。
    /// </summary>
    public void ShowWindow() {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
        _notifyIcon.Visible = false;
    }

    private static bool TryGetValidIpv4(string input, out string normalizedIp) {
        normalizedIp = string.Empty;
        if (!IPAddress.TryParse(input, out var address)) {
            return false;
        }

        if (address.AddressFamily != AddressFamily.InterNetwork) {
            return false;
        }

        normalizedIp = address.ToString();
        return true;
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

    /// <summary>
    /// 保存配置到文件
    /// Save configuration to file
    /// </summary>
    private bool SaveConfiguration() {
        try {
            // 验证 IP 地址
            var ip = textIPAddress.Text.Trim();
            if (!TryGetValidIpv4(ip, out var validatedIP)) {
                MessageBox.Show(@"IP 地址格式不正确", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textIPAddress.Focus();
                return false;
            }

            // 更新配置数据
            _appConfig.Data.TargetIP = validatedIP;
            _appConfig.Data.TargetBluetoothName = textBluetoothName.Text.Trim();

            // 验证并保存超时时间
            if (int.TryParse(textOfflineSecs.Text.Trim(), out int offlineSecs) && offlineSecs > 0) {
                _appConfig.Data.OfflineTimeoutSeconds = offlineSecs;
            } else {
                MessageBox.Show(@"设备离线超时必须是正整数", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textOfflineSecs.Focus();
                return false;
            }

            if (int.TryParse(textInactiveSecs.Text.Trim(), out int inactiveSecs) && inactiveSecs > 0) {
                _appConfig.Data.InactiveTimeoutSeconds = inactiveSecs;
            } else {
                MessageBox.Show(@"设备空闲超时必须是正整数", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textInactiveSecs.Focus();
                return false;
            }

            _appConfig.Data.WeChatWebhookKey = textWeChatKey.Text.Trim();
            _appConfig.Data.TelegramBotToken = textTelegramToken.Text.Trim();
            _appConfig.Data.TelegramChatId = textTelegramChatId.Text.Trim();
            _appConfig.Data.AutoStartEnabled = cbAutoStart.Checked;

            // 保存到文件
            _appConfig.Save();

            Logger.Info("Configuration saved successfully");
            return true;
        } catch (Exception ex) {
            Logger.Error("Failed to save configuration", ex);
            MessageBox.Show($@"保存配置失败: {ex.Message}", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void textBluetoothName_TextChanged(object sender, EventArgs e) {

    }

    private void textOfflineSecs_TextChanged(object sender, EventArgs e) {

    }

    private void textInactiveSecs_TextChanged(object sender, EventArgs e) {

    }
}
