using Carrot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Carrot.AutoLock;

/// <summary>
/// 通知管理器，统一管理多个通知渠道
/// Notification manager, manages multiple notification channels
/// </summary>
public class NotificationManager : IDisposable {
    private readonly List<INotifier> _notifiers = new();
    private bool _disposed;

    /// <summary>
    /// 添加通知器
    /// Add notifier
    /// </summary>
    public void AddNotifier(INotifier notifier) {
        if (notifier != null && notifier.IsConfigured) {
            _notifiers.Add(notifier);
            Logger.Info($"Added notifier: {notifier.Name}");
        }
    }

    /// <summary>
    /// 移除通知器
    /// Remove notifier
    /// </summary>
    public void RemoveNotifier(INotifier notifier) {
        if (_notifiers.Remove(notifier)) {
            Logger.Info($"Removed notifier: {notifier.Name}");
        }
    }

    /// <summary>
    /// 清空所有通知器
    /// Clear all notifiers
    /// </summary>
    public void ClearNotifiers() {
        _notifiers.Clear();
        Logger.Info("Cleared all notifiers");
    }

    /// <summary>
    /// 获取已配置的通知器数量
    /// Get configured notifier count
    /// </summary>
    public int ConfiguredCount => _notifiers.Count(n => n.IsConfigured);

    /// <summary>
    /// 向所有已配置的通知器发送文本消息（并行发送，不阻塞）
    /// Send text message to all configured notifiers (parallel, non-blocking)
    /// </summary>
    /// <param name="content">消息内容 / Message content</param>
    public void SendMessage(string content) {
        if (_notifiers.Count == 0) {
            Logger.Warning("No notifiers configured, skip notification");
            return;
        }

        // 异步发送，不阻塞主流程
        _ = Task.Run(async () => {
            var tasks = _notifiers
                .Where(n => n.IsConfigured)
                .Select(n => n.SendMessageAsync(content));

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);
            Logger.Info($"Notification sent: {successCount}/{results.Length} succeeded");
        });
    }

    /// <summary>
    /// 向所有已配置的通知器发送 Markdown 消息（并行发送，不阻塞）
    /// Send Markdown message to all configured notifiers (parallel, non-blocking)
    /// </summary>
    /// <param name="content">Markdown 内容 / Markdown content</param>
    public void SendMarkdown(string content) {
        if (_notifiers.Count == 0) {
            Logger.Warning("No notifiers configured, skip notification");
            return;
        }

        // 异步发送，不阻塞主流程
        _ = Task.Run(async () => {
            var tasks = _notifiers
                .Where(n => n.IsConfigured)
                .Select(n => n.SendMarkdownAsync(content));

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r);
            Logger.Info($"Markdown notification sent: {successCount}/{results.Length} succeeded");
        });
    }

    /// <summary>
    /// 发送锁定通知（使用 Markdown 格式）
    /// Send lock notification (using Markdown format)
    /// </summary>
    /// <param name="deviceInfo">设备信息（IP 或蓝牙名称） / Device info (IP or Bluetooth name)</param>
    /// <param name="reason">锁定原因 / Lock reason</param>
    public void SendLockNotification(string deviceInfo, string reason) {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var content = $"🔒 电脑即将锁定\n\n" +
                      $"设备: {deviceInfo}\n" +
                      $"原因: {reason}\n" +
                      $"时间: {timestamp}\n\n" +
                      $"_Carrot.AutoLock 自动锁定提醒_";

        SendMessage(content);
    }

    public void Dispose() {
        if (_disposed) return;

        // 释放 HTTP 客户端资源
        foreach (var notifier in _notifiers) {
            if (notifier is IDisposable disposable) {
                disposable.Dispose();
            }
        }

        _notifiers.Clear();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
