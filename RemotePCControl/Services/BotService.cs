using System.Text;
using System.Text.Json;
using RemotePCControl.Models;

namespace RemotePCControl.Services;

public interface IBotService
{
    Task Notify(string message);
    Task StartListening(Func<string, Task<string>> onCommand);
    void StopListening();
}

public class BotService : IBotService
{
    private readonly IConfiguration _config;
    private readonly ILogger<BotService> _logger;
    private readonly HttpClient _http;
    private BotSettings? _settings;
    private CancellationTokenSource? _cts;

    public BotService(IConfiguration config, ILogger<BotService> logger, IHttpClientFactory factory)
    {
        _config = config;
        _logger = logger;
        _http = factory.CreateClient();
        _http.Timeout = TimeSpan.FromSeconds(15);
        _settings = config.GetSection("Bots").Get<BotSettings>();
    }

    public async Task Notify(string message)
    {
        if (_settings?.NotifyOnOperation != true) return;
        var fullMsg = $"[{Environment.MachineName}] {message}";
        var tasks = new List<Task>();
        if (_settings.WeWork.Enabled && !string.IsNullOrEmpty(_settings.WeWork.WebhookUrl))
            tasks.Add(SendWeWork(_settings.WeWork.WebhookUrl, fullMsg));
        if (_settings.WeChat.Enabled && !string.IsNullOrEmpty(_settings.WeChat.WebhookUrl))
            tasks.Add(SendWeWork(_settings.WeChat.WebhookUrl, fullMsg));
        if (_settings.Telegram.Enabled && !string.IsNullOrEmpty(_settings.Telegram.BotToken))
            tasks.Add(SendTelegram(_settings.Telegram.BotToken, fullMsg));
        try { await Task.WhenAll(tasks); }
        catch (Exception ex) { _logger.LogError(ex, "Notify failed"); }
    }

    private async Task SendWeWork(string url, string message)
    {
        try
        {
            var payload = new { msgtype = "text", text = new { content = message } };
            var json = JsonSerializer.Serialize(payload);
            var resp = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            _logger.LogInformation("WeWork message sent");
        }
        catch (Exception ex) { _logger.LogError(ex, "WeWork failed"); }
    }

    private async Task SendTelegram(string token, string message)
    {
        try
        {
            var ids = (_settings?.Telegram.AllowedChatIds ?? string.Empty)
                .Split(',', ';', ' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var id in ids)
            {
                var url = $"https://api.telegram.org/bot{token}/sendMessage";
                var payload = new { chat_id = id.Trim(), text = message };
                var json = JsonSerializer.Serialize(payload);
                var resp = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
                resp.EnsureSuccessStatusCode();
            }
            _logger.LogInformation("Telegram message sent");
        }
        catch (Exception ex) { _logger.LogError(ex, "Telegram failed"); }
    }

    public async Task StartListening(Func<string, Task<string>> onCommand)
    {
        if (_settings?.Telegram.Enabled != true || string.IsNullOrEmpty(_settings.Telegram.BotToken))
        {
            _logger.LogInformation("Telegram bot disabled - skip listening");
            return;
        }
        _cts = new CancellationTokenSource();
        var token = _settings.Telegram.BotToken;
        var allowedIds = (_settings.Telegram.AllowedChatIds ?? string.Empty)
            .Split(',', ';', ' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToHashSet();

        var offset = 0L;
        _logger.LogInformation("Telegram bot listening started");
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{token}/getUpdates?timeout=30&offset={offset}";
                var respText = await _http.GetStringAsync(url, _cts.Token);
                using var doc = JsonDocument.Parse(respText);
                if (!doc.RootElement.TryGetProperty("result", out var results)) continue;
                foreach (var upd in results.EnumerateArray())
                {
                    var updateId = upd.GetProperty("update_id").GetInt64();
                    offset = updateId + 1;
                    if (!upd.TryGetProperty("message", out var msg)) continue;
                    var chatId = msg.GetProperty("chat").GetProperty("id").ToString();
                    var text = msg.GetProperty("text").GetString() ?? string.Empty;
                    if (allowedIds.Count > 0 && !allowedIds.Contains(chatId))
                    {
                        _logger.LogWarning("Blocked chat id {chatId}", chatId);
                        await SendTelegramRaw(token, chatId, "未授权的聊天 ID");
                        continue;
                    }
                    _logger.LogInformation("Telegram command: {cmd}", text);
                    var reply = await onCommand(text);
                    await SendTelegramRaw(token, chatId, reply);
                }
            }
            catch (TaskCanceledException) when (_cts.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telegram polling error");
                await Task.Delay(3000, _cts.Token);
            }
        }
        _logger.LogInformation("Telegram bot stopped");
    }

    private async Task SendTelegramRaw(string token, string chatId, string message)
    {
        try
        {
            var url = $"https://api.telegram.org/bot{token}/sendMessage";
            var payload = new { chat_id = chatId, text = message };
            var json = JsonSerializer.Serialize(payload);
            await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        }
        catch (Exception ex) { _logger.LogError(ex, "Telegram send failed"); }
    }

    public void StopListening() => _cts?.Cancel();
}
