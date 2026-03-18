using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Carrot.Common;

namespace Carrot.AutoLock;

/// <summary>
/// 企业微信机器人通知器
/// WeChat Work robot notifier
/// </summary>
public class WeChatNotifier : INotifier {
    private const string BaseUrl = "https://qyapi.weixin.qq.com/cgi-bin/webhook/send";
    private readonly string _webhookKey;
    private readonly HttpClient _httpClient;

    public string Name => "企业微信";
    public bool IsConfigured => !string.IsNullOrWhiteSpace(_webhookKey);

    /// <summary>
    /// 初始化企业微信通知器
    /// Initialize WeChat notifier
    /// </summary>
    /// <param name="webhookKey">机器人 Key / Robot Key</param>
    public WeChatNotifier(string webhookKey) {
        _webhookKey = webhookKey?.Trim() ?? string.Empty;
        _httpClient = new HttpClient {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    /// <summary>
    /// 发送文本消息
    /// Send text message
    /// </summary>
    public async Task<bool> SendMessageAsync(string content) {
        if (!IsConfigured) {
            Logger.Warning("WeChat notifier not configured");
            return false;
        }

        try {
            var url = $"{BaseUrl}?key={_webhookKey}";
            var payload = new {
                msgtype = "text",
                text = new {
                    content = content
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) {
                Logger.Info($"WeChat notification sent successfully: {result}");
                return true;
            } else {
                Logger.Warning($"WeChat notification failed: {response.StatusCode} - {result}");
                return false;
            }
        } catch (Exception ex) {
            Logger.Error("Failed to send WeChat notification", ex);
            return false;
        }
    }

    /// <summary>
    /// 发送 Markdown 消息
    /// Send Markdown message
    /// </summary>
    public async Task<bool> SendMarkdownAsync(string content) {
        if (!IsConfigured) {
            Logger.Warning("WeChat notifier not configured");
            return false;
        }

        try {
            var url = $"{BaseUrl}?key={_webhookKey}";
            var payload = new {
                msgtype = "markdown",
                markdown = new {
                    content = content
                }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, httpContent);
            var result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode) {
                Logger.Info($"WeChat Markdown notification sent successfully: {result}");
                return true;
            } else {
                Logger.Warning($"WeChat Markdown notification failed: {response.StatusCode} - {result}");
                return false;
            }
        } catch (Exception ex) {
            Logger.Error("Failed to send WeChat Markdown notification", ex);
            return false;
        }
    }
}
