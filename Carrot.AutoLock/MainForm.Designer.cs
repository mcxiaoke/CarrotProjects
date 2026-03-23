namespace Carrot.AutoLock {
    partial class MainForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            textIPAddress = new TextBox();
            btnStart = new Button();
            btnExit = new Button();
            cbAutoStart = new CheckBox();
            btnViewLog = new Button();
            InfoText = new TextBox();
            labelIP = new Label();
            labelBluetooth = new Label();
            textBluetoothName = new TextBox();
            labelWeChat = new Label();
            textWeChatKey = new TextBox();
            labelTelegram = new Label();
            textTelegramToken = new TextBox();
            labelTelegramChat = new Label();
            textTelegramChatId = new TextBox();
            offlineLabel = new Label();
            inactiveLabel = new Label();
            textOfflineSecs = new TextBox();
            textInactiveSecs = new TextBox();
            SuspendLayout();
            // 
            // textIPAddress
            // 
            textIPAddress.Font = new Font("Microsoft YaHei UI", 12F);
            textIPAddress.Location = new Point(49, 60);
            textIPAddress.Margin = new Padding(4);
            textIPAddress.MaxLength = 16;
            textIPAddress.Name = "textIPAddress";
            textIPAddress.Size = new Size(388, 38);
            textIPAddress.TabIndex = 0;
            textIPAddress.Text = "192.168.1.";
            textIPAddress.TextChanged += TextIPAddress_TextChanged;
            // 
            // btnStart
            // 
            btnStart.Font = new Font("Microsoft YaHei UI", 11F);
            btnStart.Location = new Point(49, 560);
            btnStart.Margin = new Padding(4);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(122, 47);
            btnStart.TabIndex = 5;
            btnStart.Text = "启动";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += BtnStart_Click;
            // 
            // btnExit
            // 
            btnExit.Font = new Font("Microsoft YaHei UI", 11F);
            btnExit.Location = new Point(684, 560);
            btnExit.Margin = new Padding(4);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(122, 47);
            btnExit.TabIndex = 8;
            btnExit.Text = "退出";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += BtnExit_Click;
            // 
            // cbAutoStart
            // 
            cbAutoStart.AutoSize = true;
            cbAutoStart.Font = new Font("Microsoft YaHei UI", 10F);
            cbAutoStart.Location = new Point(49, 627);
            cbAutoStart.Margin = new Padding(4);
            cbAutoStart.Name = "cbAutoStart";
            cbAutoStart.Size = new Size(118, 31);
            cbAutoStart.TabIndex = 7;
            cbAutoStart.Text = "开机启动";
            cbAutoStart.UseVisualStyleBackColor = true;
            cbAutoStart.CheckedChanged += CbAutoStart_CheckedChanged;
            // 
            // btnViewLog
            // 
            btnViewLog.Font = new Font("Microsoft YaHei UI", 11F);
            btnViewLog.Location = new Point(196, 560);
            btnViewLog.Margin = new Padding(4);
            btnViewLog.Name = "btnViewLog";
            btnViewLog.Size = new Size(122, 47);
            btnViewLog.TabIndex = 6;
            btnViewLog.Text = "日志";
            btnViewLog.UseVisualStyleBackColor = true;
            btnViewLog.Click += BtnViewLog_Click;
            // 
            // InfoText
            // 
            InfoText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            InfoText.Location = new Point(49, 680);
            InfoText.Margin = new Padding(4);
            InfoText.Multiline = true;
            InfoText.Name = "InfoText";
            InfoText.ReadOnly = true;
            InfoText.Size = new Size(757, 132);
            InfoText.TabIndex = 9;
            InfoText.TabStop = false;
            // 
            // labelIP
            // 
            labelIP.AutoSize = true;
            labelIP.Font = new Font("Microsoft YaHei UI", 10F);
            labelIP.Location = new Point(49, 20);
            labelIP.Margin = new Padding(4, 0, 4, 0);
            labelIP.Name = "labelIP";
            labelIP.Size = new Size(110, 27);
            labelIP.TabIndex = 100;
            labelIP.Text = "目标IP地址";
            // 
            // labelBluetooth
            // 
            labelBluetooth.AutoSize = true;
            labelBluetooth.Font = new Font("Microsoft YaHei UI", 10F);
            labelBluetooth.Location = new Point(445, 20);
            labelBluetooth.Margin = new Padding(4, 0, 4, 0);
            labelBluetooth.Name = "labelBluetooth";
            labelBluetooth.Size = new Size(132, 27);
            labelBluetooth.TabIndex = 101;
            labelBluetooth.Text = "目标蓝牙名称";
            // 
            // textBluetoothName
            // 
            textBluetoothName.Font = new Font("Microsoft YaHei UI", 12F);
            textBluetoothName.Location = new Point(445, 60);
            textBluetoothName.Margin = new Padding(4);
            textBluetoothName.Name = "textBluetoothName";
            textBluetoothName.Size = new Size(361, 38);
            textBluetoothName.TabIndex = 1;
            textBluetoothName.TextChanged += textBluetoothName_TextChanged;
            // 
            // labelWeChat
            // 
            labelWeChat.AutoSize = true;
            labelWeChat.Font = new Font("Microsoft YaHei UI", 10F);
            labelWeChat.Location = new Point(49, 233);
            labelWeChat.Margin = new Padding(4, 0, 4, 0);
            labelWeChat.Name = "labelWeChat";
            labelWeChat.Size = new Size(187, 27);
            labelWeChat.TabIndex = 102;
            labelWeChat.Text = "企业微信机器人Key";
            // 
            // textWeChatKey
            // 
            textWeChatKey.Font = new Font("Microsoft YaHei UI", 12F);
            textWeChatKey.Location = new Point(49, 273);
            textWeChatKey.Margin = new Padding(4);
            textWeChatKey.Name = "textWeChatKey";
            textWeChatKey.Size = new Size(757, 38);
            textWeChatKey.TabIndex = 2;
            textWeChatKey.UseSystemPasswordChar = true;
            // 
            // labelTelegram
            // 
            labelTelegram.AutoSize = true;
            labelTelegram.Font = new Font("Microsoft YaHei UI", 10F);
            labelTelegram.Location = new Point(49, 340);
            labelTelegram.Margin = new Padding(4, 0, 4, 0);
            labelTelegram.Name = "labelTelegram";
            labelTelegram.Size = new Size(204, 27);
            labelTelegram.TabIndex = 103;
            labelTelegram.Text = "Telegram Bot Token";
            // 
            // textTelegramToken
            // 
            textTelegramToken.Font = new Font("Microsoft YaHei UI", 12F);
            textTelegramToken.Location = new Point(49, 380);
            textTelegramToken.Margin = new Padding(4);
            textTelegramToken.Name = "textTelegramToken";
            textTelegramToken.Size = new Size(757, 38);
            textTelegramToken.TabIndex = 3;
            textTelegramToken.UseSystemPasswordChar = true;
            // 
            // labelTelegramChat
            // 
            labelTelegramChat.AutoSize = true;
            labelTelegramChat.Font = new Font("Microsoft YaHei UI", 10F);
            labelTelegramChat.Location = new Point(49, 447);
            labelTelegramChat.Margin = new Padding(4, 0, 4, 0);
            labelTelegramChat.Name = "labelTelegramChat";
            labelTelegramChat.Size = new Size(177, 27);
            labelTelegramChat.TabIndex = 104;
            labelTelegramChat.Text = "Telegram Chat ID";
            // 
            // textTelegramChatId
            // 
            textTelegramChatId.Font = new Font("Microsoft YaHei UI", 12F);
            textTelegramChatId.Location = new Point(49, 487);
            textTelegramChatId.Margin = new Padding(4);
            textTelegramChatId.Name = "textTelegramChatId";
            textTelegramChatId.Size = new Size(757, 38);
            textTelegramChatId.TabIndex = 4;
            // 
            // offlineLabel
            // 
            offlineLabel.AutoSize = true;
            offlineLabel.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 134);
            offlineLabel.Location = new Point(49, 123);
            offlineLabel.Name = "offlineLabel";
            offlineLabel.Size = new Size(192, 27);
            offlineLabel.TabIndex = 105;
            offlineLabel.Text = "设备离线超时（秒）";
            // 
            // inactiveLabel
            // 
            inactiveLabel.AutoSize = true;
            inactiveLabel.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 134);
            inactiveLabel.Location = new Point(445, 123);
            inactiveLabel.Name = "inactiveLabel";
            inactiveLabel.Size = new Size(192, 27);
            inactiveLabel.TabIndex = 106;
            inactiveLabel.Text = "设备空闲超时（秒）";
            // 
            // textOfflineSecs
            // 
            textOfflineSecs.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            textOfflineSecs.Location = new Point(51, 163);
            textOfflineSecs.Name = "textOfflineSecs";
            textOfflineSecs.Size = new Size(388, 38);
            textOfflineSecs.TabIndex = 107;
            textOfflineSecs.TextChanged += textOfflineSecs_TextChanged;
            // 
            // textInactiveSecs
            // 
            textInactiveSecs.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 134);
            textInactiveSecs.Location = new Point(445, 163);
            textInactiveSecs.Name = "textInactiveSecs";
            textInactiveSecs.Size = new Size(361, 38);
            textInactiveSecs.TabIndex = 108;
            textInactiveSecs.TextChanged += textInactiveSecs_TextChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(856, 847);
            Controls.Add(textInactiveSecs);
            Controls.Add(textOfflineSecs);
            Controls.Add(inactiveLabel);
            Controls.Add(offlineLabel);
            Controls.Add(InfoText);
            Controls.Add(btnViewLog);
            Controls.Add(cbAutoStart);
            Controls.Add(btnExit);
            Controls.Add(btnStart);
            Controls.Add(textTelegramChatId);
            Controls.Add(labelTelegramChat);
            Controls.Add(textTelegramToken);
            Controls.Add(labelTelegram);
            Controls.Add(textWeChatKey);
            Controls.Add(labelWeChat);
            Controls.Add(textBluetoothName);
            Controls.Add(labelBluetooth);
            Controls.Add(textIPAddress);
            Controls.Add(labelIP);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MinimumSize = new Size(875, 861);
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

        private Label labelIP;
        private TextBox textIPAddress;
        private Label labelBluetooth;
        private TextBox textBluetoothName;
        private Label labelWeChat;
        private TextBox textWeChatKey;
        private Label labelTelegram;
        private TextBox textTelegramToken;
        private Label labelTelegramChat;
        private TextBox textTelegramChatId;
        private Button btnStart;
        private CheckBox cbAutoStart;
        private Button btnExit;
        private Button btnViewLog;
        private TextBox InfoText;
        private Label offlineLabel;
        private Label inactiveLabel;
        private TextBox textOfflineSecs;
        private TextBox textInactiveSecs;
    }
}
