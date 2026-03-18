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
            this.textIPAddress = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.cbAutoStart = new System.Windows.Forms.CheckBox();
            this.btnViewLog = new System.Windows.Forms.Button();
            this.InfoText = new System.Windows.Forms.TextBox();
            this.labelIP = new System.Windows.Forms.Label();
            this.labelBluetooth = new System.Windows.Forms.Label();
            this.textBluetoothName = new System.Windows.Forms.TextBox();
            this.labelWeChat = new System.Windows.Forms.Label();
            this.textWeChatKey = new System.Windows.Forms.TextBox();
            this.labelTelegram = new System.Windows.Forms.Label();
            this.textTelegramToken = new System.Windows.Forms.TextBox();
            this.labelTelegramChat = new System.Windows.Forms.Label();
            this.textTelegramChatId = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelIP
            // 
            this.labelIP.AutoSize = true;
            this.labelIP.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.labelIP.Location = new System.Drawing.Point(40, 15);
            this.labelIP.Name = "labelIP";
            this.labelIP.Size = new System.Drawing.Size(120, 27);
            this.labelIP.TabIndex = 100;
            this.labelIP.Text = "目标IP地址";
            // 
            // textIPAddress
            // 
            this.textIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textIPAddress.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textIPAddress.Location = new System.Drawing.Point(40, 45);
            this.textIPAddress.MaxLength = 16;
            this.textIPAddress.Name = "textIPAddress";
            this.textIPAddress.Size = new System.Drawing.Size(620, 39);
            this.textIPAddress.TabIndex = 0;
            this.textIPAddress.Text = "192.168.1.";
            this.textIPAddress.TextChanged += new System.EventHandler(this.TextIPAddress_TextChanged);
            // 
            // labelBluetooth
            // 
            this.labelBluetooth.AutoSize = true;
            this.labelBluetooth.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.labelBluetooth.Location = new System.Drawing.Point(40, 95);
            this.labelBluetooth.Name = "labelBluetooth";
            this.labelBluetooth.Size = new System.Drawing.Size(130, 27);
            this.labelBluetooth.TabIndex = 101;
            this.labelBluetooth.Text = "蓝牙设备名称";
            // 
            // textBluetoothName
            // 
            this.textBluetoothName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBluetoothName.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textBluetoothName.Location = new System.Drawing.Point(40, 125);
            this.textBluetoothName.Name = "textBluetoothName";
            this.textBluetoothName.Size = new System.Drawing.Size(620, 39);
            this.textBluetoothName.TabIndex = 1;
            // 
            // labelWeChat
            // 
            this.labelWeChat.AutoSize = true;
            this.labelWeChat.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.labelWeChat.Location = new System.Drawing.Point(40, 175);
            this.labelWeChat.Name = "labelWeChat";
            this.labelWeChat.Size = new System.Drawing.Size(180, 27);
            this.labelWeChat.TabIndex = 102;
            this.labelWeChat.Text = "企业微信机器人Key";
            // 
            // textWeChatKey
            // 
            this.textWeChatKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textWeChatKey.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textWeChatKey.Location = new System.Drawing.Point(40, 205);
            this.textWeChatKey.Name = "textWeChatKey";
            this.textWeChatKey.Size = new System.Drawing.Size(620, 39);
            this.textWeChatKey.TabIndex = 2;
            this.textWeChatKey.UseSystemPasswordChar = true;
            // 
            // labelTelegram
            // 
            this.labelTelegram.AutoSize = true;
            this.labelTelegram.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.labelTelegram.Location = new System.Drawing.Point(40, 255);
            this.labelTelegram.Name = "labelTelegram";
            this.labelTelegram.Size = new System.Drawing.Size(140, 27);
            this.labelTelegram.TabIndex = 103;
            this.labelTelegram.Text = "Telegram Bot Token";
            // 
            // textTelegramToken
            // 
            this.textTelegramToken.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTelegramToken.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textTelegramToken.Location = new System.Drawing.Point(40, 285);
            this.textTelegramToken.Name = "textTelegramToken";
            this.textTelegramToken.Size = new System.Drawing.Size(620, 39);
            this.textTelegramToken.TabIndex = 3;
            this.textTelegramToken.UseSystemPasswordChar = true;
            // 
            // labelTelegramChat
            // 
            this.labelTelegramChat.AutoSize = true;
            this.labelTelegramChat.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.labelTelegramChat.Location = new System.Drawing.Point(40, 335);
            this.labelTelegramChat.Name = "labelTelegramChat";
            this.labelTelegramChat.Size = new System.Drawing.Size(160, 27);
            this.labelTelegramChat.TabIndex = 104;
            this.labelTelegramChat.Text = "Telegram Chat ID";
            // 
            // textTelegramChatId
            // 
            this.textTelegramChatId.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textTelegramChatId.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textTelegramChatId.Location = new System.Drawing.Point(40, 365);
            this.textTelegramChatId.Name = "textTelegramChatId";
            this.textTelegramChatId.Size = new System.Drawing.Size(620, 39);
            this.textTelegramChatId.TabIndex = 4;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnStart.Location = new System.Drawing.Point(40, 420);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(100, 35);
            this.btnStart.TabIndex = 5;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnViewLog
            // 
            this.btnViewLog.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnViewLog.Location = new System.Drawing.Point(160, 420);
            this.btnViewLog.Name = "btnViewLog";
            this.btnViewLog.Size = new System.Drawing.Size(100, 35);
            this.btnViewLog.TabIndex = 6;
            this.btnViewLog.Text = "日志";
            this.btnViewLog.UseVisualStyleBackColor = true;
            this.btnViewLog.Click += new System.EventHandler(this.BtnViewLog_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnExit.Location = new System.Drawing.Point(560, 420);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(100, 35);
            this.btnExit.TabIndex = 8;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // cbAutoStart
            // 
            this.cbAutoStart.AutoSize = true;
            this.cbAutoStart.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.cbAutoStart.Location = new System.Drawing.Point(40, 470);
            this.cbAutoStart.Name = "cbAutoStart";
            this.cbAutoStart.Size = new System.Drawing.Size(112, 31);
            this.cbAutoStart.TabIndex = 7;
            this.cbAutoStart.Text = "开机启动";
            this.cbAutoStart.UseVisualStyleBackColor = true;
            this.cbAutoStart.CheckedChanged += new System.EventHandler(this.CbAutoStart_CheckedChanged);
            // 
            // InfoText
            // 
            this.InfoText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.InfoText.Location = new System.Drawing.Point(40, 510);
            this.InfoText.Multiline = true;
            this.InfoText.Name = "InfoText";
            this.InfoText.ReadOnly = true;
            this.InfoText.Size = new System.Drawing.Size(620, 100);
            this.InfoText.TabIndex = 9;
            this.InfoText.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 635);
            this.Controls.Add(this.InfoText);
            this.Controls.Add(this.btnViewLog);
            this.Controls.Add(this.cbAutoStart);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.textTelegramChatId);
            this.Controls.Add(this.labelTelegramChat);
            this.Controls.Add(this.textTelegramToken);
            this.Controls.Add(this.labelTelegram);
            this.Controls.Add(this.textWeChatKey);
            this.Controls.Add(this.labelWeChat);
            this.Controls.Add(this.textBluetoothName);
            this.Controls.Add(this.labelBluetooth);
            this.Controls.Add(this.textIPAddress);
            this.Controls.Add(this.labelIP);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(720, 660);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CarrotLock";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
