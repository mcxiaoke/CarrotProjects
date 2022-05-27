namespace GenshinNotifier {
    partial class OptionForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.CommonGroup = new System.Windows.Forms.GroupBox();
            this.OptionCloseConfirm = new System.Windows.Forms.CheckBox();
            this.OptionHideToTray = new System.Windows.Forms.CheckBox();
            this.OptionAutoStart = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.OptionRemindTransformer = new System.Windows.Forms.CheckBox();
            this.OptionRemindDiscount = new System.Windows.Forms.CheckBox();
            this.OptionRemindCoin = new System.Windows.Forms.CheckBox();
            this.OptionRemindExpedition = new System.Windows.Forms.CheckBox();
            this.OptionRemindTask = new System.Windows.Forms.CheckBox();
            this.OptionRemindResin = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.OptionAutoUpdate = new System.Windows.Forms.CheckBox();
            this.OptionEnableNotifications = new System.Windows.Forms.CheckBox();
            this.OptionCheckinOnStart = new System.Windows.Forms.CheckBox();
            this.OptionRefreshOnStart = new System.Windows.Forms.CheckBox();
            this.ProjectLabel = new System.Windows.Forms.LinkLabel();
            this.CloseButton = new System.Windows.Forms.Button();
            this.CheckButton = new System.Windows.Forms.Button();
            this.CommonGroup.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // CommonGroup
            // 
            this.CommonGroup.AutoSize = true;
            this.CommonGroup.Controls.Add(this.OptionCloseConfirm);
            this.CommonGroup.Controls.Add(this.OptionHideToTray);
            this.CommonGroup.Controls.Add(this.OptionAutoStart);
            this.CommonGroup.Location = new System.Drawing.Point(13, 13);
            this.CommonGroup.Name = "CommonGroup";
            this.CommonGroup.Size = new System.Drawing.Size(335, 223);
            this.CommonGroup.TabIndex = 0;
            this.CommonGroup.TabStop = false;
            this.CommonGroup.Text = "通用";
            // 
            // OptionCloseConfirm
            // 
            this.OptionCloseConfirm.AutoSize = true;
            this.OptionCloseConfirm.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionCloseConfirm;
            this.OptionCloseConfirm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionCloseConfirm.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionCloseConfirm", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionCloseConfirm.Location = new System.Drawing.Point(11, 145);
            this.OptionCloseConfirm.Margin = new System.Windows.Forms.Padding(8);
            this.OptionCloseConfirm.Name = "OptionCloseConfirm";
            this.OptionCloseConfirm.Size = new System.Drawing.Size(190, 35);
            this.OptionCloseConfirm.TabIndex = 2;
            this.OptionCloseConfirm.Text = "退出时需确认";
            this.OptionCloseConfirm.UseVisualStyleBackColor = true;
            // 
            // OptionHideToTray
            // 
            this.OptionHideToTray.AutoSize = true;
            this.OptionHideToTray.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionHideToTray;
            this.OptionHideToTray.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionHideToTray.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionHideToTray", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionHideToTray.Location = new System.Drawing.Point(11, 94);
            this.OptionHideToTray.Margin = new System.Windows.Forms.Padding(8);
            this.OptionHideToTray.Name = "OptionHideToTray";
            this.OptionHideToTray.Size = new System.Drawing.Size(238, 35);
            this.OptionHideToTray.TabIndex = 1;
            this.OptionHideToTray.Text = "最小化到系统托盘";
            this.OptionHideToTray.UseVisualStyleBackColor = true;
            // 
            // OptionAutoStart
            // 
            this.OptionAutoStart.AutoSize = true;
            this.OptionAutoStart.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionAutoStart;
            this.OptionAutoStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionAutoStart.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionAutoStart", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionAutoStart.Location = new System.Drawing.Point(11, 43);
            this.OptionAutoStart.Margin = new System.Windows.Forms.Padding(8);
            this.OptionAutoStart.Name = "OptionAutoStart";
            this.OptionAutoStart.Size = new System.Drawing.Size(190, 35);
            this.OptionAutoStart.TabIndex = 0;
            this.OptionAutoStart.Text = "跟随系统启动";
            this.OptionAutoStart.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.OptionRemindTransformer);
            this.groupBox1.Controls.Add(this.OptionRemindDiscount);
            this.groupBox1.Controls.Add(this.OptionRemindCoin);
            this.groupBox1.Controls.Add(this.OptionRemindExpedition);
            this.groupBox1.Controls.Add(this.OptionRemindTask);
            this.groupBox1.Controls.Add(this.OptionRemindResin);
            this.groupBox1.Location = new System.Drawing.Point(354, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(346, 223);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "提醒项目";
            // 
            // OptionRemindTransformer
            // 
            this.OptionRemindTransformer.AutoSize = true;
            this.OptionRemindTransformer.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionRemindTransformer;
            this.OptionRemindTransformer.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionRemindTransformer.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionRemindTransformer", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionRemindTransformer.Location = new System.Drawing.Point(169, 145);
            this.OptionRemindTransformer.Margin = new System.Windows.Forms.Padding(8);
            this.OptionRemindTransformer.Name = "OptionRemindTransformer";
            this.OptionRemindTransformer.Size = new System.Drawing.Size(166, 35);
            this.OptionRemindTransformer.TabIndex = 5;
            this.OptionRemindTransformer.Text = "参量质变仪";
            this.OptionRemindTransformer.UseVisualStyleBackColor = true;
            // 
            // OptionRemindDiscount
            // 
            this.OptionRemindDiscount.AutoSize = true;
            this.OptionRemindDiscount.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionRemindDiscount;
            this.OptionRemindDiscount.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionRemindDiscount.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionRemindDiscount", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionRemindDiscount.Location = new System.Drawing.Point(169, 94);
            this.OptionRemindDiscount.Margin = new System.Windows.Forms.Padding(8);
            this.OptionRemindDiscount.Name = "OptionRemindDiscount";
            this.OptionRemindDiscount.Size = new System.Drawing.Size(142, 35);
            this.OptionRemindDiscount.TabIndex = 4;
            this.OptionRemindDiscount.Text = "减半周本";
            this.OptionRemindDiscount.UseVisualStyleBackColor = true;
            // 
            // OptionRemindCoin
            // 
            this.OptionRemindCoin.AutoSize = true;
            this.OptionRemindCoin.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionRemindCoin;
            this.OptionRemindCoin.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionRemindCoin", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionRemindCoin.Location = new System.Drawing.Point(169, 43);
            this.OptionRemindCoin.Margin = new System.Windows.Forms.Padding(8);
            this.OptionRemindCoin.Name = "OptionRemindCoin";
            this.OptionRemindCoin.Size = new System.Drawing.Size(142, 35);
            this.OptionRemindCoin.TabIndex = 3;
            this.OptionRemindCoin.Text = "洞天宝钱";
            this.OptionRemindCoin.UseVisualStyleBackColor = true;
            // 
            // OptionRemindExpedition
            // 
            this.OptionRemindExpedition.AutoSize = true;
            this.OptionRemindExpedition.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionRemindExpedition;
            this.OptionRemindExpedition.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionRemindExpedition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionRemindExpedition.Location = new System.Drawing.Point(11, 145);
            this.OptionRemindExpedition.Margin = new System.Windows.Forms.Padding(8);
            this.OptionRemindExpedition.Name = "OptionRemindExpedition";
            this.OptionRemindExpedition.Size = new System.Drawing.Size(142, 35);
            this.OptionRemindExpedition.TabIndex = 2;
            this.OptionRemindExpedition.Text = "探索派遣";
            this.OptionRemindExpedition.UseVisualStyleBackColor = true;
            // 
            // OptionRemindTask
            // 
            this.OptionRemindTask.AutoSize = true;
            this.OptionRemindTask.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionRemindTask;
            this.OptionRemindTask.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionRemindTask.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionRemindTask", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionRemindTask.Location = new System.Drawing.Point(11, 94);
            this.OptionRemindTask.Margin = new System.Windows.Forms.Padding(8);
            this.OptionRemindTask.Name = "OptionRemindTask";
            this.OptionRemindTask.Size = new System.Drawing.Size(142, 35);
            this.OptionRemindTask.TabIndex = 1;
            this.OptionRemindTask.Text = "每日委托";
            this.OptionRemindTask.UseVisualStyleBackColor = true;
            // 
            // OptionRemindResin
            // 
            this.OptionRemindResin.AutoSize = true;
            this.OptionRemindResin.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionRemindResin;
            this.OptionRemindResin.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionRemindResin.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionRemindResin", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionRemindResin.Location = new System.Drawing.Point(11, 43);
            this.OptionRemindResin.Margin = new System.Windows.Forms.Padding(8);
            this.OptionRemindResin.Name = "OptionRemindResin";
            this.OptionRemindResin.Size = new System.Drawing.Size(142, 35);
            this.OptionRemindResin.TabIndex = 0;
            this.OptionRemindResin.Text = "原粹树脂";
            this.OptionRemindResin.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.AutoSize = true;
            this.groupBox2.Controls.Add(this.OptionAutoUpdate);
            this.groupBox2.Controls.Add(this.OptionEnableNotifications);
            this.groupBox2.Controls.Add(this.OptionCheckinOnStart);
            this.groupBox2.Controls.Add(this.OptionRefreshOnStart);
            this.groupBox2.Location = new System.Drawing.Point(13, 243);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(676, 172);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "其它";
            // 
            // OptionAutoUpdate
            // 
            this.OptionAutoUpdate.AutoSize = true;
            this.OptionAutoUpdate.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionAutoUpdate;
            this.OptionAutoUpdate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionAutoUpdate.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionAutoUpdate", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionAutoUpdate.Location = new System.Drawing.Point(341, 94);
            this.OptionAutoUpdate.Margin = new System.Windows.Forms.Padding(8);
            this.OptionAutoUpdate.Name = "OptionAutoUpdate";
            this.OptionAutoUpdate.Size = new System.Drawing.Size(214, 35);
            this.OptionAutoUpdate.TabIndex = 3;
            this.OptionAutoUpdate.Text = "自动检查新版本";
            this.OptionAutoUpdate.UseVisualStyleBackColor = true;
            // 
            // OptionEnableNotifications
            // 
            this.OptionEnableNotifications.AutoSize = true;
            this.OptionEnableNotifications.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionEnableNotifications;
            this.OptionEnableNotifications.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionEnableNotifications.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionEnableNotifications", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionEnableNotifications.Location = new System.Drawing.Point(341, 43);
            this.OptionEnableNotifications.Margin = new System.Windows.Forms.Padding(8);
            this.OptionEnableNotifications.Name = "OptionEnableNotifications";
            this.OptionEnableNotifications.Size = new System.Drawing.Size(295, 35);
            this.OptionEnableNotifications.TabIndex = 2;
            this.OptionEnableNotifications.Text = "启用Windows系统通知";
            this.OptionEnableNotifications.UseVisualStyleBackColor = true;
            // 
            // OptionCheckinOnStart
            // 
            this.OptionCheckinOnStart.AutoSize = true;
            this.OptionCheckinOnStart.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionCheckinOnStart;
            this.OptionCheckinOnStart.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionCheckinOnStart", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionCheckinOnStart.Location = new System.Drawing.Point(11, 94);
            this.OptionCheckinOnStart.Margin = new System.Windows.Forms.Padding(8);
            this.OptionCheckinOnStart.Name = "OptionCheckinOnStart";
            this.OptionCheckinOnStart.Size = new System.Drawing.Size(262, 35);
            this.OptionCheckinOnStart.TabIndex = 1;
            this.OptionCheckinOnStart.Text = "每天米游社自动签到";
            this.OptionCheckinOnStart.UseVisualStyleBackColor = true;
            // 
            // OptionRefreshOnStart
            // 
            this.OptionRefreshOnStart.AutoSize = true;
            this.OptionRefreshOnStart.Checked = global::GenshinNotifier.Properties.Settings.Default.OptionRefreshOnStart;
            this.OptionRefreshOnStart.CheckState = System.Windows.Forms.CheckState.Checked;
            this.OptionRefreshOnStart.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::GenshinNotifier.Properties.Settings.Default, "OptionRefreshOnStart", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.OptionRefreshOnStart.Location = new System.Drawing.Point(11, 43);
            this.OptionRefreshOnStart.Margin = new System.Windows.Forms.Padding(8);
            this.OptionRefreshOnStart.Name = "OptionRefreshOnStart";
            this.OptionRefreshOnStart.Size = new System.Drawing.Size(262, 35);
            this.OptionRefreshOnStart.TabIndex = 0;
            this.OptionRefreshOnStart.Text = "打开时自动刷新数据";
            this.OptionRefreshOnStart.UseVisualStyleBackColor = true;
            // 
            // ProjectLabel
            // 
            this.ProjectLabel.Location = new System.Drawing.Point(7, 426);
            this.ProjectLabel.Margin = new System.Windows.Forms.Padding(3);
            this.ProjectLabel.Name = "ProjectLabel";
            this.ProjectLabel.Padding = new System.Windows.Forms.Padding(6);
            this.ProjectLabel.Size = new System.Drawing.Size(682, 43);
            this.ProjectLabel.TabIndex = 0;
            this.ProjectLabel.TabStop = true;
            this.ProjectLabel.Text = "前往项目页面";
            this.ProjectLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ProjectLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ProjectLabel_LinkClicked);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(527, 491);
            this.CloseButton.Margin = new System.Windows.Forms.Padding(8);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Padding = new System.Windows.Forms.Padding(8);
            this.CloseButton.Size = new System.Drawing.Size(150, 61);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "关闭";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // CheckButton
            // 
            this.CheckButton.Location = new System.Drawing.Point(17, 491);
            this.CheckButton.Margin = new System.Windows.Forms.Padding(8);
            this.CheckButton.Name = "CheckButton";
            this.CheckButton.Padding = new System.Windows.Forms.Padding(8);
            this.CheckButton.Size = new System.Drawing.Size(150, 61);
            this.CheckButton.TabIndex = 4;
            this.CheckButton.Text = "检查更新";
            this.CheckButton.UseVisualStyleBackColor = true;
            this.CheckButton.Click += new System.EventHandler(this.CheckButton_Click);
            // 
            // OptionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 569);
            this.Controls.Add(this.CheckButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.ProjectLabel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.CommonGroup);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "选项";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptionForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OptionForm_FormClosed);
            this.Load += new System.EventHandler(this.OptionForm_Load);
            this.Shown += new System.EventHandler(this.OptionForm_Shown);
            this.CommonGroup.ResumeLayout(false);
            this.CommonGroup.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox CommonGroup;
        private System.Windows.Forms.CheckBox OptionCloseConfirm;
        private System.Windows.Forms.CheckBox OptionHideToTray;
        private System.Windows.Forms.CheckBox OptionAutoStart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox OptionRemindExpedition;
        private System.Windows.Forms.CheckBox OptionRemindTask;
        private System.Windows.Forms.CheckBox OptionRemindResin;
        private System.Windows.Forms.CheckBox OptionRemindTransformer;
        private System.Windows.Forms.CheckBox OptionRemindDiscount;
        private System.Windows.Forms.CheckBox OptionRemindCoin;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox OptionCheckinOnStart;
        private System.Windows.Forms.CheckBox OptionRefreshOnStart;
        private System.Windows.Forms.LinkLabel ProjectLabel;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.CheckBox OptionEnableNotifications;
        private System.Windows.Forms.CheckBox OptionAutoUpdate;
        private System.Windows.Forms.Button CheckButton;
    }
}