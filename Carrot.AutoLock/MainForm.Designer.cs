namespace Carrot.AutoLock {
    partial class MainForm {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            btnStart = new Button();
            btnExit = new Button();
            cbAutoStart = new CheckBox();
            cbAutoLock = new CheckBox();
            btnViewLog = new Button();
            InfoText = new TextBox();
            ConfigText = new TextBox();
            btnSettings = new Button();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Font = new Font("Microsoft YaHei UI", 11F);
            btnStart.Location = new Point(22, 253);
            btnStart.Margin = new Padding(4);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(162, 40);
            btnStart.TabIndex = 0;
            btnStart.Text = "启动服务";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += BtnStart_Click;
            // 
            // btnExit
            // 
            btnExit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExit.Font = new Font("Microsoft YaHei UI", 11F);
            btnExit.Location = new Point(634, 253);
            btnExit.Margin = new Padding(4);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(120, 40);
            btnExit.TabIndex = 3;
            btnExit.Text = "退出";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += BtnExit_Click;
            // 
            // cbAutoStart
            // 
            cbAutoStart.AutoSize = true;
            cbAutoStart.Font = new Font("Microsoft YaHei UI", 10F);
            cbAutoStart.Location = new Point(24, 301);
            cbAutoStart.Margin = new Padding(4);
            cbAutoStart.Name = "cbAutoStart";
            cbAutoStart.Size = new Size(118, 31);
            cbAutoStart.TabIndex = 4;
            cbAutoStart.Text = "开机启动";
            cbAutoStart.UseVisualStyleBackColor = true;
            cbAutoStart.CheckedChanged += CbAutoStart_CheckedChanged;
            // 
            // cbAutoLock
            // 
            cbAutoLock.AutoSize = true;
            cbAutoLock.Font = new Font("Microsoft YaHei UI", 10F);
            cbAutoLock.Location = new Point(170, 301);
            cbAutoLock.Margin = new Padding(4);
            cbAutoLock.Name = "cbAutoLock";
            cbAutoLock.Size = new Size(118, 31);
            cbAutoLock.TabIndex = 7;
            cbAutoLock.Text = "自动锁屏";
            cbAutoLock.UseVisualStyleBackColor = true;
            cbAutoLock.CheckedChanged += CbAutoLock_CheckedChanged;
            // 
            // btnViewLog
            // 
            btnViewLog.Font = new Font("Microsoft YaHei UI", 11F);
            btnViewLog.Location = new Point(192, 253);
            btnViewLog.Margin = new Padding(4);
            btnViewLog.Name = "btnViewLog";
            btnViewLog.Size = new Size(120, 40);
            btnViewLog.TabIndex = 1;
            btnViewLog.Text = "日志";
            btnViewLog.UseVisualStyleBackColor = true;
            btnViewLog.Click += BtnViewLog_Click;
            // 
            // InfoText
            // 
            InfoText.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            InfoText.Font = new Font("Microsoft YaHei UI", 10F);
            InfoText.Location = new Point(24, 354);
            InfoText.Margin = new Padding(4);
            InfoText.Multiline = true;
            InfoText.Name = "InfoText";
            InfoText.ReadOnly = true;
            InfoText.Size = new Size(730, 135);
            InfoText.TabIndex = 5;
            InfoText.TabStop = false;
            // 
            // ConfigText
            // 
            ConfigText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ConfigText.Font = new Font("Microsoft YaHei UI", 10F);
            ConfigText.Location = new Point(24, 27);
            ConfigText.Margin = new Padding(4);
            ConfigText.Multiline = true;
            ConfigText.Name = "ConfigText";
            ConfigText.ReadOnly = true;
            ConfigText.Size = new Size(730, 204);
            ConfigText.TabIndex = 6;
            ConfigText.TabStop = false;
            ConfigText.TextChanged += ConfigText_TextChanged;
            // 
            // btnSettings
            // 
            btnSettings.Font = new Font("Microsoft YaHei UI", 11F);
            btnSettings.Location = new Point(320, 253);
            btnSettings.Margin = new Padding(4);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(120, 40);
            btnSettings.TabIndex = 2;
            btnSettings.Text = "设置";
            btnSettings.UseVisualStyleBackColor = true;
            btnSettings.Click += BtnSettings_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(778, 504);
            Controls.Add(btnSettings);
            Controls.Add(ConfigText);
            Controls.Add(InfoText);
            Controls.Add(btnViewLog);
            Controls.Add(cbAutoLock);
            Controls.Add(cbAutoStart);
            Controls.Add(btnExit);
            Controls.Add(btnStart);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MinimumSize = new Size(800, 560);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Carrot AutoLock";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Resize += MainForm_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStart;
        private CheckBox cbAutoStart;
        private CheckBox cbAutoLock;
        private Button btnExit;
        private Button btnViewLog;
        private TextBox InfoText;
        private TextBox ConfigText;
        private Button btnSettings;
    }
}
