using System.Text.Json;

namespace MonitorControlTray;

/// <summary>
/// 配置编辑器窗体，支持增删改模式和时间设置，保存到 config.json
/// </summary>
public class ConfigEditorForm : Form {
    private readonly string configPath;
    private AppConfig config;
    private string currentMode;
    private bool isLoading = true;

    private ListBox modeListBox = null!;
    private DataGridView settingsGrid = null!;
    private TextBox switchToGameModeTextBox = null!;
    private TextBox switchToDailyModeTextBox = null!;
    private TextBox manualRefreshTextBox = null!;
    private TextBox increaseBrightnessTextBox = null!;
    private TextBox decreaseBrightnessTextBox = null!;

    public ConfigEditorForm(string configPath, AppConfig config, string currentMode) {
        this.configPath = configPath;
        this.config = config;
        this.currentMode = currentMode;
        InitializeComponent();
        LoadModes();
        LoadHotkeys();
        isLoading = false;
    }

    private void InitializeComponent() {
        SuspendLayout();

        Text = "配置编辑器";
        Size = new Size(620, 520);
        MinimumSize = new Size(480, 400);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.Sizable;

        // ===== 整体用 TableLayoutPanel 布局，避免遮挡 =====
        var mainLayout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = SystemColors.Control
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));  // 左侧模式列表
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));    // 右侧设置区
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // ===== 左侧：模式列表 =====
        var leftPanel = new Panel {
            Dock = DockStyle.Fill,
            Padding = new Padding(6, 6, 2, 6)
        };

        var modeLabel = new Label {
            Text = "模式列表",
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(2),
            Font = new Font(Font, FontStyle.Bold)
        };

        modeListBox = new ListBox {
            Dock = DockStyle.Fill,
            IntegralHeight = false
        };
        modeListBox.SelectedIndexChanged += ModeListBox_SelectedIndexChanged;

        var addModeBtn = new Button {
            Text = "添加模式",
            Dock = DockStyle.Bottom,
            Height = 28
        };
        addModeBtn.Click += AddModeBtn_Click;

        var deleteModeBtn = new Button {
            Text = "删除模式",
            Dock = DockStyle.Bottom,
            Height = 28
        };
        deleteModeBtn.Click += DeleteModeBtn_Click;

        leftPanel.Controls.AddRange([modeListBox, deleteModeBtn, addModeBtn, modeLabel]);
        // Dock 顺序：后加的在底层，所以 modeLabel 在最上，modeListBox 填中间

        // ===== 右侧：时间设置 + 快捷键 =====
        var rightPanel = new Panel {
            Dock = DockStyle.Fill,
            Padding = new Padding(2, 6, 6, 6)
        };

        // --- 时间设置标签 ---
        var settingsLabel = new Label {
            Text = "时间设置",
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(2),
            Font = new Font(Font, FontStyle.Bold)
        };

        // --- 添加行/删除行 工具条 ---
        var gridToolbar = new Panel {
            Dock = DockStyle.Bottom,
            Height = 30
        };

        var addRowBtn = new Button {
            Text = "添加配置",
            Size = new Size(80, 26),
            Location = new Point(0, 2)
        };
        addRowBtn.Click += AddRowBtn_Click;

        var deleteRowBtn = new Button {
            Text = "删除配置",
            Size = new Size(80, 26),
            Location = new Point(86, 2)
        };
        deleteRowBtn.Click += DeleteRowBtn_Click;

        gridToolbar.Controls.AddRange([addRowBtn, deleteRowBtn]);

        // --- 时间设置表格 ---
        settingsGrid = new DataGridView {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.Fixed3D,
            RowHeadersVisible = false,
            RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { Font = new Font(Font, FontStyle.Bold) },
            DefaultCellStyle = new DataGridViewCellStyle { Padding = new Padding(2, 0, 2, 0) }
        };

        var timeCol = new DataGridViewTextBoxColumn {
            HeaderText = "时间",
            Name = "Time",
            Width = 70
        };

        var brightnessCol = new DataGridViewTextBoxColumn {
            HeaderText = "亮度",
            Name = "Brightness",
            Width = 60
        };

        var contrastCol = new DataGridViewTextBoxColumn {
            HeaderText = "对比度",
            Name = "Contrast",
            Width = 60
        };

        settingsGrid.Columns.AddRange([timeCol, brightnessCol, contrastCol]);
        settingsGrid.CellValidating += SettingsGrid_CellValidating;
        settingsGrid.CellEndEdit += SettingsGrid_CellEndEdit;

        // --- 快捷键设置区域 ---
        var hotkeyPanel = new TableLayoutPanel {
            Dock = DockStyle.Bottom,
            Height = 140,
            ColumnCount = 2,
            RowCount = 5,
            AutoSize = false
        };
        hotkeyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        hotkeyPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 5; i++) {
            hotkeyPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        }

        var hotkeyLabel = new Label {
            Text = "快捷键设置",
            AutoSize = true,
            Dock = DockStyle.Bottom,
            Padding = new Padding(2, 6, 2, 2),
            Font = new Font(Font, FontStyle.Bold)
        };

        switchToGameModeTextBox = CreateHotkeyRow(hotkeyPanel, 0, "切换游戏模式");
        switchToDailyModeTextBox = CreateHotkeyRow(hotkeyPanel, 1, "切换日常模式");
        manualRefreshTextBox = CreateHotkeyRow(hotkeyPanel, 2, "手动刷新");
        increaseBrightnessTextBox = CreateHotkeyRow(hotkeyPanel, 3, "增加亮度");
        decreaseBrightnessTextBox = CreateHotkeyRow(hotkeyPanel, 4, "降低亮度");

        // 右侧面板组装（Dock 顺序：后加的在底层）
        rightPanel.Controls.Add(settingsGrid);        // Fill - 占据剩余空间
        rightPanel.Controls.Add(gridToolbar);          // Bottom - 表格下方工具条
        rightPanel.Controls.Add(hotkeyPanel);          // Bottom - 快捷键区域
        rightPanel.Controls.Add(hotkeyLabel);          // Bottom - 快捷键标签
        rightPanel.Controls.Add(settingsLabel);        // Top - 时间设置标签

        // ===== 底部按钮 =====
        var bottomPanel = new Panel {
            Dock = DockStyle.Bottom,
            Height = 44,
            Padding = new Padding(8, 6, 8, 6)
        };

        var saveBtn = new Button {
            Text = "保存",
            Size = new Size(80, 30),
            Anchor = AnchorStyles.Right
        };
        saveBtn.Click += SaveBtn_Click;

        var cancelBtn = new Button {
            Text = "取消",
            Size = new Size(80, 30),
            Anchor = AnchorStyles.Right
        };
        cancelBtn.Click += (_, _) => Close();

        bottomPanel.Controls.Add(cancelBtn);
        bottomPanel.Controls.Add(saveBtn);
        bottomPanel.Resize += (_, _) => {
            cancelBtn.Location = new Point(bottomPanel.Width - 180, 8);
            saveBtn.Location = new Point(bottomPanel.Width - 90, 8);
        };

        // ===== 主窗体组装 =====
        Controls.Add(mainLayout);
        Controls.Add(bottomPanel);  // Bottom 先加，确保不被 Fill 遮挡

        mainLayout.Controls.Add(leftPanel, 0, 0);
        mainLayout.Controls.Add(rightPanel, 1, 0);

        ResumeLayout(true);
    }

    private static TextBox CreateHotkeyRow(TableLayoutPanel panel, int row, string labelText) {
        var label = new Label {
            Text = labelText,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        var textBox = new TextBox {
            Dock = DockStyle.Fill,
            PlaceholderText = "如: Ctrl+Shift+G"
        };

        panel.Controls.Add(label, 0, row);
        panel.Controls.Add(textBox, 1, row);
        return textBox;
    }

    #region 模式列表操作

    private void LoadModes() {
        modeListBox.Items.Clear();
        foreach (var mode in config.Modes.Keys) {
            modeListBox.Items.Add(mode);
        }

        int idx = modeListBox.Items.IndexOf(currentMode);
        modeListBox.SelectedIndex = idx >= 0 ? idx : 0;
    }

    private void ModeListBox_SelectedIndexChanged(object? sender, EventArgs e) {
        if (!isLoading) {
            SaveCurrentGridData();
        }

        string? selected = modeListBox.SelectedItem?.ToString();
        if (selected is null) return;

        currentMode = selected;
        LoadSettingsGrid(selected);
    }

    private void AddModeBtn_Click(object? sender, EventArgs e) {
        using var dialog = new Form {
            Text = "添加模式",
            Size = new Size(300, 130),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var label = new Label {
            Text = "模式名称：",
            Location = new Point(20, 20),
            AutoSize = true
        };

        var textBox = new TextBox {
            Location = new Point(100, 17),
            Size = new Size(160, 25)
        };

        var okBtn = new Button {
            Text = "确定",
            Location = new Point(100, 55),
            Size = new Size(75, 28),
            DialogResult = DialogResult.OK
        };

        var cancelDlgBtn = new Button {
            Text = "取消",
            Location = new Point(185, 55),
            Size = new Size(75, 28),
            DialogResult = DialogResult.Cancel
        };

        dialog.Controls.AddRange([label, textBox, okBtn, cancelDlgBtn]);
        dialog.AcceptButton = okBtn;
        dialog.CancelButton = cancelDlgBtn;

        if (dialog.ShowDialog(this) == DialogResult.OK) {
            string name = textBox.Text.Trim();
            if (string.IsNullOrEmpty(name)) {
                MessageBox.Show("模式名称不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (config.Modes.ContainsKey(name)) {
                MessageBox.Show($"模式 '{name}' 已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            config.Modes[name] = [new TimeSetting { Time = "08:00", Brightness = 50, Contrast = 50 }];
            modeListBox.Items.Add(name);
            modeListBox.SelectedItem = name;
        }
    }

    private void DeleteModeBtn_Click(object? sender, EventArgs e) {
        string? selected = modeListBox.SelectedItem?.ToString();
        if (selected is null) return;

        if (config.Modes.Count <= 1) {
            MessageBox.Show("至少保留一个模式", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"确定删除模式 '{selected}'？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
            config.Modes.Remove(selected);
            modeListBox.Items.Remove(selected);

            if (modeListBox.Items.Count > 0) {
                modeListBox.SelectedIndex = 0;
            }
        }
    }

    #endregion

    #region 时间设置表格操作

    private void LoadSettingsGrid(string modeName) {
        settingsGrid.Rows.Clear();

        if (!config.Modes.TryGetValue(modeName, out var settings)) return;

        foreach (var s in settings) {
            settingsGrid.Rows.Add(s.Time, s.Brightness, s.Contrast);
        }
    }

    private void SaveCurrentGridData() {
        if (string.IsNullOrEmpty(currentMode) || !config.Modes.ContainsKey(currentMode)) return;

        var settings = new List<TimeSetting>();
        foreach (DataGridViewRow row in settingsGrid.Rows) {
            if (row.IsNewRow) continue;

            string? time = row.Cells["Time"].Value?.ToString();
            int brightness = int.TryParse(row.Cells["Brightness"].Value?.ToString(), out int b) ? b : 50;
            int contrast = int.TryParse(row.Cells["Contrast"].Value?.ToString(), out int c) ? c : 50;

            if (!string.IsNullOrEmpty(time)) {
                settings.Add(new TimeSetting {
                    Time = time,
                    Brightness = Math.Clamp(brightness, 0, 100),
                    Contrast = Math.Clamp(contrast, 0, 100)
                });
            }
        }

        config.Modes[currentMode] = settings;
    }

    private void AddRowBtn_Click(object? sender, EventArgs e) {
        settingsGrid.Rows.Add("08:00", 50, 50);
        // 选中新添加的行
        if (settingsGrid.Rows.Count > 0) {
            settingsGrid.CurrentCell = settingsGrid.Rows[^1].Cells[0];
        }
    }

    private void DeleteRowBtn_Click(object? sender, EventArgs e) {
        if (settingsGrid.Rows.Count == 0) return;

        var rowsToDelete = new List<DataGridViewRow>();
        if (settingsGrid.SelectedRows.Count > 0) {
            foreach (DataGridViewRow row in settingsGrid.SelectedRows) {
                if (!row.IsNewRow) rowsToDelete.Add(row);
            }
        } else if (settingsGrid.CurrentRow is not null && !settingsGrid.CurrentRow.IsNewRow) {
            rowsToDelete.Add(settingsGrid.CurrentRow);
        }

        foreach (var row in rowsToDelete) {
            settingsGrid.Rows.Remove(row);
        }
    }

    private void SettingsGrid_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e) {
        if (e.RowIndex < 0) return;

        var grid = sender as DataGridView ?? throw new InvalidOperationException();
        var cell = grid.Rows[e.RowIndex].Cells[e.ColumnIndex];

        switch (grid.Columns[e.ColumnIndex].Name) {
            case "Time": {
                    string? val = e.FormattedValue?.ToString();
                    if (!TimeSpan.TryParse(val, out _)) {
                        cell.ErrorText = "格式错误，请用 HH:mm";
                        e.Cancel = true;
                    } else {
                        cell.ErrorText = string.Empty;
                    }
                    break;
                }
            case "Brightness":
            case "Contrast": {
                    if (int.TryParse(e.FormattedValue?.ToString(), out int val)) {
                        if (val < 0 || val > 100) {
                            cell.ErrorText = "0-100";
                            e.Cancel = true;
                        } else {
                            cell.ErrorText = string.Empty;
                        }
                    } else {
                        cell.ErrorText = "请输入数字";
                        e.Cancel = true;
                    }
                    break;
                }
        }
    }

    private void SettingsGrid_CellEndEdit(object? sender, DataGridViewCellEventArgs e) {
        if (e.RowIndex < 0) return;
        settingsGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].ErrorText = string.Empty;
    }

    #endregion

    #region 快捷键操作

    private void LoadHotkeys() {
        if (config.Hotkeys is null) return;

        switchToGameModeTextBox.Text = config.Hotkeys.SwitchToGameMode ?? "";
        switchToDailyModeTextBox.Text = config.Hotkeys.SwitchToDailyMode ?? "";
        manualRefreshTextBox.Text = config.Hotkeys.ManualRefresh ?? "";
        increaseBrightnessTextBox.Text = config.Hotkeys.IncreaseBrightness ?? "";
        decreaseBrightnessTextBox.Text = config.Hotkeys.DecreaseBrightness ?? "";
    }

    private void SaveHotkeys() {
        config.Hotkeys ??= new HotkeyConfig();
        config.Hotkeys.SwitchToGameMode = string.IsNullOrEmpty(switchToGameModeTextBox.Text) ? null : switchToGameModeTextBox.Text.Trim();
        config.Hotkeys.SwitchToDailyMode = string.IsNullOrEmpty(switchToDailyModeTextBox.Text) ? null : switchToDailyModeTextBox.Text.Trim();
        config.Hotkeys.ManualRefresh = string.IsNullOrEmpty(manualRefreshTextBox.Text) ? null : manualRefreshTextBox.Text.Trim();
        config.Hotkeys.IncreaseBrightness = string.IsNullOrEmpty(increaseBrightnessTextBox.Text) ? null : increaseBrightnessTextBox.Text.Trim();
        config.Hotkeys.DecreaseBrightness = string.IsNullOrEmpty(decreaseBrightnessTextBox.Text) ? null : decreaseBrightnessTextBox.Text.Trim();
    }

    #endregion

    #region 保存配置

    private void SaveBtn_Click(object? sender, EventArgs e) {
        SaveCurrentGridData();
        SaveHotkeys();

        var errors = new List<string>();
        foreach (var (modeName, settings) in config.Modes) {
            if (settings is null || settings.Count == 0) {
                errors.Add($"模式 '{modeName}' 没有时间设置");
                continue;
            }

            foreach (var s in settings) {
                if (!TimeSpan.TryParse(s.Time, out _)) {
                    errors.Add($"模式 '{modeName}' 时间格式错误: {s.Time}");
                }
                if (s.Brightness is < 0 or > 100) {
                    errors.Add($"模式 '{modeName}' 亮度超出范围: {s.Brightness}");
                }
                if (s.Contrast is < 0 or > 100) {
                    errors.Add($"模式 '{modeName}' 对比度超出范围: {s.Contrast}");
                }
            }
        }

        if (errors.Count > 0) {
            MessageBox.Show("配置验证失败:\n" + string.Join("\n", errors), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(configPath, json);
            Program.Log("配置已保存");

            MessageBox.Show("配置已保存，将在主程序中自动重载", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        } catch (Exception ex) {
            MessageBox.Show($"保存配置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    #endregion
}
