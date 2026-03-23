using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Carrot.Common;

namespace Carrot.AutoLock;

/// <summary>
/// Telegram 机器人通知器
/// Telegram bot notifier
/// </summary>
public class TelegramNotifier : INotifier {
    private const string BaseUrl = "https://api.telegram.org/bot";
    private readonly string _botToken;
    private readonly string _chatId;
    private readonly HttpClient _httpClient;

    public string Name => "Telegram";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_botToken) && !string.IsNullOrWhiteSpace(_chatId);

    /// <summary>
    /// 初始化 Telegram 通知器
    /// Initialize Telegram notifier
    /// </summary>
    /// <param name="botToken">Bot Token</param>
    /// <param name="chatId">Chat ID（用户或群组 ID） / Chat ID (user or group ID)</param>
    /// <param name="proxyUrl">代理地址（可选） / Proxy URL (optional)</param>
    public TelegramNotifier(string botToken, string chatId, string? proxyUrl = null) {
        _botToken = botToken?.Trim() ?? string.Empty;
        _chatId = chatId?.Trim() ?? string.Empty;

        var handler = new HttpClientHandler();
        if (!string.IsNullOrEmpty(proxyUrl)) {
            handler.Proxy = new WebProxy(proxyUrl);
            handler.UseProxy = true;
        }

        _httpClient = new HttpClient(handler) {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    /// <summary>
    /// 发送文本消息
    /// Send text message
    /// </summary>
    public async Task<bool> SendMessageAsync(string content) {
        if (!IsConfigured) {
            Logger.Warning("Telegram notifier not configured");
            return false;
        }

        try {
            var url = $"{BaseUrl}{_botToken}/sendMessage";
            var payload = new {
                chat_id = _chatId,
                text = content
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) {
                Logger.Info($"Telegram notification sent successfully: {result}");
                return true;
            } else {
                Logger.Warning($"Telegram notification failed: {response.StatusCode} - {result}");
                return false;
            }
        } catch (Exception ex) {
            Logger.Error("Failed to send Telegram notification", ex);
            return false;
        }
    }

    /// <summary>
    /// 发送 Markdown 消息
    /// Send Markdown message
    /// </summary>
    public async Task<bool> SendMarkdownAsync(string content) {
        if (!IsConfigured) {
            Logger.Warning("Telegram notifier not configured");
            return false;
        }

        try {
            var url = $"{BaseUrl}{_botToken}/sendMessage";
            var payload = new {
                chat_id = _chatId,
                text = content,
                parse_mode = "Markdown"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) {
                Logger.Info($"Telegram Markdown notification sent successfully: {result}");
                return true;
            } else {
                Logger.Warning($"Telegram Markdown notification failed: {response.StatusCode} - {result}");
                return false;
            }
        } catch (Exception ex) {
            Logger.Error("Failed to send Telegram Markdown notification", ex);
            return false;
        }
    }
}
