using Carrot.Common;
using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Carrot.AutoLock;

/// <summary>
/// Main application form. Controls tray icon and menu logic.
/// �������ࡣ��������ͼ��Ͳ˵��߼���
/// </summary>
public partial class MainForm : Form {

    private const string NAME = "CarrotLock";
    private const int NotifyTextMaxLength = 63;

    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenuStrip;
    private string _deviceIP = "";
    private readonly ActiveChecker _checker;

    /// <summary>
    /// Initializes the main form and tray icon.
    /// ��ʼ�����������ͼ�ꡣ
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
        var showMenuItem = new ToolStripMenuItem("��ʾ����", null, ShowWindowMenuItem_Click);
        _contextMenuStrip.Items.Add(showMenuItem);

        // Add separator
        _contextMenuStrip.Items.Add(new ToolStripSeparator());

        // Exit menu item
        var exitMenuItem = new ToolStripMenuItem("�˳�Ӧ��", null, ExitMenuItem_Click);
        _contextMenuStrip.Items.Add(exitMenuItem);

        // Bind ContextMenuStrip to NotifyIcon
        _notifyIcon.ContextMenuStrip = _contextMenuStrip;
        _notifyIcon.Click += NotifyIcon_Click;

        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e) {
        Logger.Info(@"MainForm_Load");
        LoadConfig();
        textIPAddress.Text = _deviceIP;

        // Initialize AutoStart Checkbox
        cbAutoStart.Checked = IsAutoStartEnabled(Application.ProductName ?? NAME);

        UpdateUI();
        ToggleCheck();
    }

    /// <summary>
    /// Loads the configuration (target IP) from file.
    /// �������ļ��������� (Ŀ�� IP)��
    /// </summary>
    private void LoadConfig() {
        try {
            // AppInfo.LocalAppDataPath already includes Company/Product folders
            string path = Path.Combine(AppInfo.LocalAppDataPath, "config.txt");
            if (File.Exists(path)) {
                _deviceIP = File.ReadAllText(path).Trim();
            }
        } catch (Exception ex) {
            Logger.Error("LoadConfig", ex);
        }
        if (string.IsNullOrWhiteSpace(_deviceIP)) {
            _deviceIP = ActiveChecker.DEFAULT_IP;
        }
    }

    /// <summary>
    /// Saves the configuration (target IP) to file.
    /// �������� (Ŀ�� IP) ���ļ���
    /// </summary>
    private void SaveConfig() {
        try {
            string path = Path.Combine(AppInfo.LocalAppDataPath, "config.txt");
            File.WriteAllText(path, _deviceIP);
        } catch (Exception ex) {
            Logger.Error("SaveConfig", ex);
        }
    }

    private void MainForm_Resize(object sender, EventArgs e) {
        Logger.Info($@"MainForm_Resize {this.WindowState}");
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
        Logger.Info($@"MainForm_FormClosing Reason:{e.CloseReason}");
        // If user creates closing event, minimize to tray
        if (e.CloseReason == CloseReason.UserClosing) {
            e.Cancel = true;
            this.WindowState = FormWindowState.Minimized;
            _notifyIcon.Visible = true;
            return;
        }

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
        Logger.Info(@"ExitMenuItem_Click");
        _checker.Stop();
        Application.Exit();
    }

    private void BtnExit_Click(object sender, EventArgs e) {
        Logger.Info(@"BtnExit_Click");
        _checker.Stop();
        _checker.Callback = null;
        Application.Exit();
    }

    private void BtnStart_Click(object sender, EventArgs e) {
        Logger.Info(@"BtnStart_Click");
        ToggleCheck();
    }

    /// <summary>
    /// Toggles the checker status (Start/Stop).
    /// �л������״̬ (��ʼ/ֹͣ)��
    /// </summary>
    private void ToggleCheck() {
        if (_checker.IsRunning()) {
            _checker.Stop();
            _checker.Callback = null;
        } else {
            // Update device IP from text box
            _deviceIP = textIPAddress.Text.Trim();

            if (!TryGetValidIpv4(_deviceIP, out var validatedIP)) {
                MessageBox.Show(@"IP address format is incorrect", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _deviceIP = validatedIP;

            SaveConfig();
            _checker.SetTargetIP(_deviceIP);
            _checker.Callback = OnStatusChanged;
            _checker.Start();
        }
        UpdateUI();
    }

    /// <summary>
    /// Callback for status updates, invokes UI update.
    /// ״̬���»ص����������� UI ���¡�
    /// </summary>
    public void OnStatusChanged(string result) {
        Logger.Info("OnStatusChanged");
        if (InvokeRequired) {
            Invoke(new MethodInvoker(UpdateUI));
        } else {
            UpdateUI();
        }
    }

    /// <summary>
    /// Updates the UI based on checker status.
    /// ���ݼ����״̬���� UI��
    /// </summary>
    private void UpdateUI() {
        if (_checker == null || _notifyIcon == null) return;

        var running = _checker.IsRunning();
        textIPAddress.Enabled = !running;
        btnStart.Text = running ? "STOP" : "START";

        var textLines = new List<string> {
            $"Running: {running}",
            $"Device Online: {_checker.IsDeviceOnline()}"
        };

        // Use InfoText (TextBox) instead of labelStatus
        InfoText.Text = string.Join("\r\n", textLines);

        _notifyIcon.Text = BuildNotifyText(running);
    }

    /// <summary>
    /// Checks if auto-start is enabled.
    /// ��鿪�������Ƿ������á�
    /// </summary>
    private bool IsAutoStartEnabled(string appName) {
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
        SetAutoStart(cbAutoStart.Checked, Application.ProductName ?? NAME, Application.ExecutablePath ?? string.Empty);
    }

    /// <summary>
    /// Sets the auto-start registry key.
    /// ���ÿ�������ע����
    /// </summary>
    private void SetAutoStart(bool enable, string appName, string appPath) {
        Logger.Debug($@"SetAutoStart Enable:{enable} AppPath:{appPath}");
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
    /// ��ʾ���ڲ���������ͼ�ꡣ
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
        var text = running ? $"{NAME} - Running {_deviceIP}" : $"{NAME} - Stopped";
        text = text.Replace('\r', ' ').Replace('\n', ' ');

        if (text.Length > NotifyTextMaxLength) {
            text = text[..NotifyTextMaxLength];
        }

        return text;
    }
}
