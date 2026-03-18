using System.Threading.Tasks;

namespace Carrot.AutoLock;

/// <summary>
/// 通知器接口，定义通用的消息发送接口
/// Notification interface, defines common message sending interface
/// </summary>
public interface INotifier {
    /// <summary>
    /// 获取通知器名称
    /// Get notifier name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 检查通知器是否已配置
    /// Check if notifier is configured
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// 发送文本消息
    /// Send text message
    /// </summary>
    /// <param name="content">消息内容 / Message content</param>
    /// <returns>是否发送成功 / Whether send successfully</returns>
    Task<bool> SendMessageAsync(string content);

    /// <summary>
    /// 发送 Markdown 消息（可选实现）
    /// Send Markdown message (optional implementation)
    /// </summary>
    /// <param name="content">Markdown 内容 / Markdown content</param>
    /// <returns>是否发送成功 / Whether send successfully</returns>
    Task<bool> SendMarkdownAsync(string content);
}
