using Carrot.Common;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace Carrot.AutoLock;

public partial class SettingsForm : Form {
    private readonly AppConfig _appConfig;

    public SettingsForm(AppConfig appConfig) {
        _appConfig = appConfig;
        InitializeComponent();
    }

    private void SettingsForm_Load(object sender, EventArgs e) {
        LoadSettings();
    }

    private void LoadSettings() {
        textIPAddress.Text = _appConfig.Data.TargetIP;
        textBluetoothName.Text = _appConfig.Data.TargetBluetoothName;
        textOfflineSecs.Text = _appConfig.Data.OfflineTimeoutSeconds.ToString();
        textInactiveSecs.Text = _appConfig.Data.InactiveTimeoutSeconds.ToString();
        textRouterPassword.Text = _appConfig.Data.RouterPassword;
        textWeChatKey.Text = _appConfig.Data.WeChatWebhookKey;
        textTelegramToken.Text = _appConfig.Data.TelegramBotToken;
        textTelegramChatId.Text = _appConfig.Data.TelegramChatId;
        textWebSocketUri.Text = _appConfig.Data.WebSocketUri;
        textExemptProcesses.Text = string.Join(", ", _appConfig.Data.ExemptProcesses);
    }

    private void BtnSave_Click(object sender, EventArgs e) {
        if (SaveSettings()) {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private void BtnCancel_Click(object sender, EventArgs e) {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private bool SaveSettings() {
        try {
            var ip = textIPAddress.Text.Trim();
            if (!TryGetValidIpv4(ip, out var validatedIP)) {
                MessageBox.Show(@"IP 地址格式不正确", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textIPAddress.Focus();
                return false;
            }

            if (!int.TryParse(textOfflineSecs.Text.Trim(), out int offlineSecs) || offlineSecs <= 0) {
                MessageBox.Show(@"设备离线超时必须是正整数", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textOfflineSecs.Focus();
                return false;
            }

            if (!int.TryParse(textInactiveSecs.Text.Trim(), out int inactiveSecs) || inactiveSecs <= 0) {
                MessageBox.Show(@"设备空闲超时必须是正整数", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textInactiveSecs.Focus();
                return false;
            }

            var uri = textWebSocketUri.Text.Trim();
            if (!string.IsNullOrEmpty(uri)) {
                if (!Uri.TryCreate(uri, UriKind.Absolute, out var validatedUri)) {
                    MessageBox.Show(@"WebSocket URI 格式不正确", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textWebSocketUri.Focus();
                    return false;
                }

                if (!string.Equals(validatedUri.Scheme, "ws", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(validatedUri.Scheme, "wss", StringComparison.OrdinalIgnoreCase)) {
                    MessageBox.Show(@"WebSocket URI 必须以 ws:// 或 wss:// 开头", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    textWebSocketUri.Focus();
                    return false;
                }
            }

            _appConfig.Data.TargetIP = validatedIP;
            _appConfig.Data.TargetBluetoothName = textBluetoothName.Text.Trim();
            _appConfig.Data.OfflineTimeoutSeconds = offlineSecs;
            _appConfig.Data.InactiveTimeoutSeconds = inactiveSecs;
            _appConfig.Data.RouterPassword = textRouterPassword.Text;
            _appConfig.Data.WeChatWebhookKey = textWeChatKey.Text.Trim();
            _appConfig.Data.TelegramBotToken = textTelegramToken.Text.Trim();
            _appConfig.Data.TelegramChatId = textTelegramChatId.Text.Trim();
            _appConfig.Data.WebSocketUri = uri;

            var exemptProcessesText = textExemptProcesses.Text;
            if (!string.IsNullOrWhiteSpace(exemptProcessesText)) {
                // 只用逗号/分号作为分隔符，保留进程名内部的空格（如 "My App"）
                // 仅移除末尾的 .exe 后缀，避免误伤名称中含 .exe 子串的进程
                _appConfig.Data.ExemptProcesses = exemptProcessesText
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ? p[..^4] : p)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            } else {
                _appConfig.Data.ExemptProcesses = new();
            }

            _appConfig.Save();

            Logger.Info("Settings saved successfully");
            return true;
        } catch (Exception ex) {
            Logger.Error("Failed to save settings", ex);
            MessageBox.Show($@"保存设置失败: {ex.Message}", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
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
}
