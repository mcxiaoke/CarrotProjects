namespace Carrot.AutoLock {
    partial class SettingsForm {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent() {
            labelIP = new Label();
            textIPAddress = new TextBox();
            labelBluetooth = new Label();
            textBluetoothName = new TextBox();
            labelOffline = new Label();
            textOfflineSecs = new TextBox();
            labelInactive = new Label();
            textInactiveSecs = new TextBox();
            labelWeChat = new Label();
            textWeChatKey = new TextBox();
            labelTelegram = new Label();
            textTelegramToken = new TextBox();
            labelTelegramChat = new Label();
            textTelegramChatId = new TextBox();
            labelWebSocket = new Label();
            textWebSocketUri = new TextBox();
            labelExempt = new Label();
            textExemptProcesses = new TextBox();
            labelRouterPwd = new Label();
            textRouterPassword = new TextBox();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // labelIP
            // 
            labelIP.AutoSize = true;
            labelIP.Font = new Font("Microsoft YaHei UI", 10F);
            labelIP.Location = new Point(24, 20);
            labelIP.Margin = new Padding(4, 0, 4, 0);
            labelIP.Name = "labelIP";
            labelIP.Size = new Size(110, 27);
            labelIP.TabIndex = 0;
            labelIP.Text = "目标IP地址";
            // 
            // textIPAddress
            // 
            textIPAddress.Font = new Font("Microsoft YaHei UI", 12F);
            textIPAddress.Location = new Point(24, 55);
            textIPAddress.Margin = new Padding(4);
            textIPAddress.MaxLength = 16;
            textIPAddress.Name = "textIPAddress";
            textIPAddress.Size = new Size(300, 38);
            textIPAddress.TabIndex = 1;
            // 
            // labelBluetooth
            // 
            labelBluetooth.AutoSize = true;
            labelBluetooth.Font = new Font("Microsoft YaHei UI", 10F);
            labelBluetooth.Location = new Point(350, 20);
            labelBluetooth.Margin = new Padding(4, 0, 4, 0);
            labelBluetooth.Name = "labelBluetooth";
            labelBluetooth.Size = new Size(132, 27);
            labelBluetooth.TabIndex = 2;
            labelBluetooth.Text = "目标蓝牙名称";
            // 
            // textBluetoothName
            // 
            textBluetoothName.Font = new Font("Microsoft YaHei UI", 12F);
            textBluetoothName.Location = new Point(350, 55);
            textBluetoothName.Margin = new Padding(4);
            textBluetoothName.Name = "textBluetoothName";
            textBluetoothName.Size = new Size(300, 38);
            textBluetoothName.TabIndex = 3;
            // 
            // labelOffline
            // 
            labelOffline.AutoSize = true;
            labelOffline.Font = new Font("Microsoft YaHei UI", 10F);
            labelOffline.Location = new Point(24, 110);
            labelOffline.Margin = new Padding(4, 0, 4, 0);
            labelOffline.Name = "labelOffline";
            labelOffline.Size = new Size(192, 27);
            labelOffline.TabIndex = 4;
            labelOffline.Text = "设备离线超时（秒）";
            // 
            // textOfflineSecs
            // 
            textOfflineSecs.Font = new Font("Microsoft YaHei UI", 12F);
            textOfflineSecs.Location = new Point(24, 145);
            textOfflineSecs.Margin = new Padding(4);
            textOfflineSecs.Name = "textOfflineSecs";
            textOfflineSecs.Size = new Size(150, 38);
            textOfflineSecs.TabIndex = 5;
            // 
            // labelInactive
            // 
            labelInactive.AutoSize = true;
            labelInactive.Font = new Font("Microsoft YaHei UI", 10F);
            labelInactive.Location = new Point(200, 110);
            labelInactive.Margin = new Padding(4, 0, 4, 0);
            labelInactive.Name = "labelInactive";
            labelInactive.Size = new Size(192, 27);
            labelInactive.TabIndex = 6;
            labelInactive.Text = "设备空闲超时（秒）";
            // 
            // textInactiveSecs
            // 
            textInactiveSecs.Font = new Font("Microsoft YaHei UI", 12F);
            textInactiveSecs.Location = new Point(200, 145);
            textInactiveSecs.Margin = new Padding(4);
            textInactiveSecs.Name = "textInactiveSecs";
            textInactiveSecs.Size = new Size(150, 38);
            textInactiveSecs.TabIndex = 7;
            // 
            // labelRouterPwd
            // 
            labelRouterPwd.AutoSize = true;
            labelRouterPwd.Font = new Font("Microsoft YaHei UI", 10F);
            labelRouterPwd.Location = new Point(380, 110);
            labelRouterPwd.Margin = new Padding(4, 0, 4, 0);
            labelRouterPwd.Name = "labelRouterPwd";
            labelRouterPwd.Size = new Size(132, 27);
            labelRouterPwd.TabIndex = 8;
            labelRouterPwd.Text = "路由器密码";
            // 
            // textRouterPassword
            // 
            textRouterPassword.Font = new Font("Microsoft YaHei UI", 12F);
            textRouterPassword.Location = new Point(380, 145);
            textRouterPassword.Margin = new Padding(4);
            textRouterPassword.Name = "textRouterPassword";
            textRouterPassword.Size = new Size(270, 38);
            textRouterPassword.TabIndex = 9;
            textRouterPassword.UseSystemPasswordChar = true;
            // 
            // labelWeChat
            // 
            labelWeChat.AutoSize = true;
            labelWeChat.Font = new Font("Microsoft YaHei UI", 10F);
            labelWeChat.Location = new Point(24, 200);
            labelWeChat.Margin = new Padding(4, 0, 4, 0);
            labelWeChat.Name = "labelWeChat";
            labelWeChat.Size = new Size(187, 27);
            labelWeChat.TabIndex = 10;
            labelWeChat.Text = "企业微信机器人Key";
            // 
            // textWeChatKey
            // 
            textWeChatKey.Font = new Font("Microsoft YaHei UI", 12F);
            textWeChatKey.Location = new Point(24, 235);
            textWeChatKey.Margin = new Padding(4);
            textWeChatKey.Name = "textWeChatKey";
            textWeChatKey.Size = new Size(626, 38);
            textWeChatKey.TabIndex = 11;
            textWeChatKey.UseSystemPasswordChar = true;
            // 
            // labelTelegram
            // 
            labelTelegram.AutoSize = true;
            labelTelegram.Font = new Font("Microsoft YaHei UI", 10F);
            labelTelegram.Location = new Point(24, 290);
            labelTelegram.Margin = new Padding(4, 0, 4, 0);
            labelTelegram.Name = "labelTelegram";
            labelTelegram.Size = new Size(204, 27);
            labelTelegram.TabIndex = 12;
            labelTelegram.Text = "Telegram Bot Token";
            // 
            // textTelegramToken
            // 
            textTelegramToken.Font = new Font("Microsoft YaHei UI", 12F);
            textTelegramToken.Location = new Point(24, 325);
            textTelegramToken.Margin = new Padding(4);
            textTelegramToken.Name = "textTelegramToken";
            textTelegramToken.Size = new Size(626, 38);
            textTelegramToken.TabIndex = 13;
            textTelegramToken.UseSystemPasswordChar = true;
            // 
            // labelTelegramChat
            // 
            labelTelegramChat.AutoSize = true;
            labelTelegramChat.Font = new Font("Microsoft YaHei UI", 10F);
            labelTelegramChat.Location = new Point(24, 380);
            labelTelegramChat.Margin = new Padding(4, 0, 4, 0);
            labelTelegramChat.Name = "labelTelegramChat";
            labelTelegramChat.Size = new Size(177, 27);
            labelTelegramChat.TabIndex = 14;
            labelTelegramChat.Text = "Telegram Chat ID";
            // 
            // textTelegramChatId
            // 
            textTelegramChatId.Font = new Font("Microsoft YaHei UI", 12F);
            textTelegramChatId.Location = new Point(24, 415);
            textTelegramChatId.Margin = new Padding(4);
            textTelegramChatId.Name = "textTelegramChatId";
            textTelegramChatId.Size = new Size(300, 38);
            textTelegramChatId.TabIndex = 15;
            // 
            // labelWebSocket
            // 
            labelWebSocket.AutoSize = true;
            labelWebSocket.Font = new Font("Microsoft YaHei UI", 10F);
            labelWebSocket.Location = new Point(350, 380);
            labelWebSocket.Margin = new Padding(4, 0, 4, 0);
            labelWebSocket.Name = "labelWebSocket";
            labelWebSocket.Size = new Size(155, 27);
            labelWebSocket.TabIndex = 16;
            labelWebSocket.Text = "WebSocket URI";
            // 
            // textWebSocketUri
            // 
            textWebSocketUri.Font = new Font("Microsoft YaHei UI", 12F);
            textWebSocketUri.Location = new Point(350, 415);
            textWebSocketUri.Margin = new Padding(4);
            textWebSocketUri.Name = "textWebSocketUri";
            textWebSocketUri.Size = new Size(300, 38);
            textWebSocketUri.TabIndex = 17;
            // 
            // labelExempt
            // 
            labelExempt.AutoSize = true;
            labelExempt.Font = new Font("Microsoft YaHei UI", 10F);
            labelExempt.Location = new Point(24, 470);
            labelExempt.Margin = new Padding(4, 0, 4, 0);
            labelExempt.Name = "labelExempt";
            labelExempt.Size = new Size(220, 27);
            labelExempt.TabIndex = 18;
            labelExempt.Text = "豁免进程（逗号分隔）";
            // 
            // textExemptProcesses
            // 
            textExemptProcesses.Font = new Font("Microsoft YaHei UI", 12F);
            textExemptProcesses.Location = new Point(24, 505);
            textExemptProcesses.Margin = new Padding(4);
            textExemptProcesses.Name = "textExemptProcesses";
            textExemptProcesses.Size = new Size(626, 38);
            textExemptProcesses.TabIndex = 19;
            // 
            // btnSave
            // 
            btnSave.Font = new Font("Microsoft YaHei UI", 11F);
            btnSave.Location = new Point(24, 565);
            btnSave.Margin = new Padding(4);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(120, 45);
            btnSave.TabIndex = 20;
            btnSave.Text = "保存";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += BtnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Font = new Font("Microsoft YaHei UI", 11F);
            btnCancel.Location = new Point(160, 565);
            btnCancel.Margin = new Padding(4);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(120, 45);
            btnCancel.TabIndex = 21;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // SettingsForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(680, 630);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(textExemptProcesses);
            Controls.Add(labelExempt);
            Controls.Add(textWebSocketUri);
            Controls.Add(labelWebSocket);
            Controls.Add(textTelegramChatId);
            Controls.Add(labelTelegramChat);
            Controls.Add(textTelegramToken);
            Controls.Add(labelTelegram);
            Controls.Add(textWeChatKey);
            Controls.Add(labelWeChat);
            Controls.Add(textRouterPassword);
            Controls.Add(labelRouterPwd);
            Controls.Add(textInactiveSecs);
            Controls.Add(labelInactive);
            Controls.Add(textOfflineSecs);
            Controls.Add(labelOffline);
            Controls.Add(textBluetoothName);
            Controls.Add(labelBluetooth);
            Controls.Add(textIPAddress);
            Controls.Add(labelIP);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "设置";
            Load += SettingsForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelIP;
        private TextBox textIPAddress;
        private Label labelBluetooth;
        private TextBox textBluetoothName;
        private Label labelOffline;
        private TextBox textOfflineSecs;
        private Label labelInactive;
        private TextBox textInactiveSecs;
        private Label labelWeChat;
        private TextBox textWeChatKey;
        private Label labelTelegram;
        private TextBox textTelegramToken;
        private Label labelTelegramChat;
        private TextBox textTelegramChatId;
        private Label labelWebSocket;
        private TextBox textWebSocketUri;
        private Label labelExempt;
        private TextBox textExemptProcesses;
        private Label labelRouterPwd;
        private TextBox textRouterPassword;
        private Button btnSave;
        private Button btnCancel;
    }
}
