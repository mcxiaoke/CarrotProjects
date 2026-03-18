using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Carrot.AutoLock;

/// <summary>
/// 日志查看窗体
/// Log viewer form
/// </summary>
public class LogViewerForm : Form {
    private readonly TextBox _logTextBox;
    private readonly Button _refreshButton;
    private readonly Button _copyButton;
    private readonly Button _clearButton;
    private readonly CheckBox _autoRefreshCheckBox;
    private readonly System.Windows.Forms.Timer _autoRefreshTimer;
    private int _lastLogCount;

    public LogViewerForm() {
        // 设置窗体属性
        this.Text = "日志查看器 - Log Viewer";
        this.Size = new Size(1200, 800);
        this.MinimumSize = new Size(900, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.ShowInTaskbar = false; // 不显示在任务栏
        this.ShowIcon = false; // 不显示图标

        // 创建日志文本框
        _logTextBox = new TextBox {
            Multiline = true,
            ScrollBars = ScrollBars.Both,
            Font = new Font("Consolas", 9F),
            ReadOnly = true,
            WordWrap = true,
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ForeColor = Color.Black
        };

        // 创建按钮面板
        var buttonPanel = new Panel {
            Dock = DockStyle.Bottom,
            Height = 50,
            BackColor = SystemColors.Control
        };

        // 刷新按钮
        _refreshButton = new Button {
            Text = "刷新日志",
            Location = new Point(10, 10),
            Size = new Size(100, 30),
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        _refreshButton.Click += RefreshButton_Click;

        // 复制按钮
        _copyButton = new Button {
            Text = "复制全部",
            Location = new Point(120, 10),
            Size = new Size(100, 30),
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        _copyButton.Click += CopyButton_Click;

        // 清空按钮
        _clearButton = new Button {
            Text = "清空日志",
            Location = new Point(230, 10),
            Size = new Size(100, 30),
            Font = new Font("Microsoft YaHei UI", 9F)
        };
        _clearButton.Click += ClearButton_Click;

        // 自动刷新复选框
        _autoRefreshCheckBox = new CheckBox {
            Text = "自动刷新 (1秒)",
            Location = new Point(350, 15),
            Size = new Size(130, 25),
            Font = new Font("Microsoft YaHei UI", 9F),
            Checked = true
        };
        _autoRefreshCheckBox.CheckedChanged += AutoRefreshCheckBox_CheckedChanged;

        // 添加控件到按钮面板
        buttonPanel.Controls.Add(_refreshButton);
        buttonPanel.Controls.Add(_copyButton);
        buttonPanel.Controls.Add(_clearButton);
        buttonPanel.Controls.Add(_autoRefreshCheckBox);

        // 添加控件到窗体
        this.Controls.Add(_logTextBox);
        this.Controls.Add(buttonPanel);

        // 初始化自动刷新计时器
        _autoRefreshTimer = new System.Windows.Forms.Timer {
            Interval = 1000 // 1秒刷新一次
        };
        _autoRefreshTimer.Tick += AutoRefreshTimer_Tick;

        // 窗体加载时启动
        this.Load += LogViewerForm_Load;
        this.FormClosing += LogViewerForm_FormClosing;
    }

    private void LogViewerForm_Load(object? sender, EventArgs e) {
        // 加载日志
        RefreshLog();

        // 启动自动刷新
        if (_autoRefreshCheckBox.Checked) {
            _autoRefreshTimer.Start();
        }

        // 滚动到最新日志
        ScrollToBottom();
    }

    private void LogViewerForm_FormClosing(object? sender, FormClosingEventArgs e) {
        _autoRefreshTimer.Stop();
        _autoRefreshTimer.Dispose();
    }

    private void RefreshButton_Click(object? sender, EventArgs e) {
        RefreshLog();
    }

    private void CopyButton_Click(object? sender, EventArgs e) {
        try {
            if (!string.IsNullOrEmpty(_logTextBox.Text)) {
                Clipboard.SetText(_logTextBox.Text);
                MessageBox.Show("日志已复制到剪贴板", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else {
                MessageBox.Show("日志为空，无法复制", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        } catch (Exception ex) {
            MessageBox.Show($"复制失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ClearButton_Click(object? sender, EventArgs e) {
        var result = MessageBox.Show(
            "确定要清空内存日志吗？\n\n注意：这只会清空内存中的日志，不会删除日志文件。\n此操作不可恢复！",
            "确认清空",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2
        );

        if (result == DialogResult.Yes) {
            try {
                Program.MemoryLog.Clear();
                RefreshLog();
                MessageBox.Show("内存日志已清空", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
                Carrot.Common.Logger.Error("Failed to clear memory log", ex);
                MessageBox.Show($"清空日志失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void AutoRefreshCheckBox_CheckedChanged(object? sender, EventArgs e) {
        if (_autoRefreshCheckBox.Checked) {
            _autoRefreshTimer.Start();
            RefreshLog();
        } else {
            _autoRefreshTimer.Stop();
        }
    }

    private void AutoRefreshTimer_Tick(object? sender, EventArgs e) {
        // 只在有新日志时刷新
        if (Program.MemoryLog != null && Program.MemoryLog.LineCount != _lastLogCount) {
            RefreshLog();
        }
    }

    private void RefreshLog() {
        try {
            var memoryLog = Program.MemoryLog;

            if (memoryLog == null) {
                _logTextBox.Text = "内存日志未初始化，请重启程序。";
                return;
            }

            // 更新标题显示日志数量
            this.Text = $"日志查看器 - 共 {memoryLog.LineCount} 行";

            // 获取日志列表
            var logs = memoryLog.GetLogLines(2000);
            _lastLogCount = memoryLog.LineCount;

            // 构建日志文本
            var sb = new StringBuilder();
            foreach (var log in logs) {
                sb.AppendLine($"[{log.Timestamp}] [{log.Level}] {log.Message}");
            }

            _logTextBox.Text = sb.ToString();

            // 始终滚动到底部
            ScrollToBottom();
        } catch (Exception ex) {
            Carrot.Common.Logger.Error("Failed to refresh log", ex);
            _logTextBox.Text = $"刷新日志失败: {ex.Message}";
        }
    }

    private void ScrollToBottom() {
        _logTextBox.SelectionStart = _logTextBox.TextLength;
        _logTextBox.ScrollToCaret();
    }
}
