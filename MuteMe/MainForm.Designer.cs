namespace MuteMe {
    partial class AutoMute {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblProcessName;
        private System.Windows.Forms.TextBox txtProcessName;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListBox lstProcesses;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.NumericUpDown numDelay;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuShow;
        private System.Windows.Forms.ToolStripMenuItem menuExit;

        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.lblProcessName = new System.Windows.Forms.Label();
            this.txtProcessName = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lstProcesses = new System.Windows.Forms.ListBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblDelay = new System.Windows.Forms.Label();
            this.numDelay = new System.Windows.Forms.NumericUpDown();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuShow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();

            this.lblProcessName.AutoSize = true;
            this.lblProcessName.Location = new System.Drawing.Point(20, 20);
            this.lblProcessName.Name = "lblProcessName";
            this.lblProcessName.Size = new System.Drawing.Size(80, 24);
            this.lblProcessName.TabIndex = 0;
            this.lblProcessName.Text = "进程名:";

            this.txtProcessName.Location = new System.Drawing.Point(100, 17);
            this.txtProcessName.Name = "txtProcessName";
            this.txtProcessName.Size = new System.Drawing.Size(300, 30);
            this.txtProcessName.TabIndex = 1;
            this.txtProcessName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtProcessName_KeyPress);

            this.btnAdd.Location = new System.Drawing.Point(420, 16);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(100, 32);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "添加";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.BtnAdd_Click);

            this.lstProcesses.FormattingEnabled = true;
            this.lstProcesses.ItemHeight = 24;
            this.lstProcesses.Location = new System.Drawing.Point(20, 60);
            this.lstProcesses.Name = "lstProcesses";
            this.lstProcesses.Size = new System.Drawing.Size(500, 196);
            this.lstProcesses.TabIndex = 3;

            this.btnRemove.Location = new System.Drawing.Point(420, 270);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(100, 32);
            this.btnRemove.TabIndex = 4;
            this.btnRemove.Text = "删除选中";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.BtnRemove_Click);

            this.lblDelay.AutoSize = true;
            this.lblDelay.Location = new System.Drawing.Point(20, 320);
            this.lblDelay.Name = "lblDelay";
            this.lblDelay.Size = new System.Drawing.Size(80, 24);
            this.lblDelay.TabIndex = 5;
            this.lblDelay.Text = "延迟时间:";

            this.numDelay.Location = new System.Drawing.Point(100, 317);
            this.numDelay.Maximum = new decimal(new int[] { 60, 0, 0, 0 });
            this.numDelay.Name = "numDelay";
            this.numDelay.Size = new System.Drawing.Size(80, 30);
            this.numDelay.TabIndex = 6;
            this.numDelay.Value = new decimal(new int[] { 3, 0, 0, 0 });
            this.numDelay.ValueChanged += new System.EventHandler(this.NumDelay_ValueChanged);

            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 370);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(100, 24);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "状态: 运行中";

            this.btnMinimize.Location = new System.Drawing.Point(420, 370);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(100, 32);
            this.btnMinimize.TabIndex = 8;
            this.btnMinimize.Text = "最小化到托盘";
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.BtnMinimize_Click);

            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Text = "MuteMe";
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);

            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuShow,
                this.menuExit
            });
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(153, 56);

            this.menuShow.Name = "menuShow";
            this.menuShow.Size = new System.Drawing.Size(152, 24);
            this.menuShow.Text = "显示主窗口";
            this.menuShow.Click += new System.EventHandler(this.MenuShow_Click);

            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(152, 24);
            this.menuExit.Text = "退出程序";
            this.menuExit.Click += new System.EventHandler(this.MenuExit_Click);

            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(550, 420);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.numDelay);
            this.Controls.Add(this.lblDelay);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.lstProcesses);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtProcessName);
            this.Controls.Add(this.lblProcessName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "AutoMute";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MuteMe";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AutoMute_FormClosing);
            this.Load += new System.EventHandler(this.AutoMute_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
