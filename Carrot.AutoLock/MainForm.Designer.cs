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
            this.lbInputTitle = new System.Windows.Forms.Label();
            this.cbAutoStart = new System.Windows.Forms.CheckBox();
            this.InfoText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textIPAddress
            // 
            this.textIPAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textIPAddress.Font = new System.Drawing.Font("Microsoft YaHei UI", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textIPAddress.Location = new System.Drawing.Point(40, 66);
            this.textIPAddress.Margin = new System.Windows.Forms.Padding(2);
            this.textIPAddress.MaxLength = 16;
            this.textIPAddress.Name = "textIPAddress";
            this.textIPAddress.Size = new System.Drawing.Size(510, 43);
            this.textIPAddress.TabIndex = 0;
            this.textIPAddress.Text = "192.168.1.";
            this.textIPAddress.TextChanged += new System.EventHandler(this.TextIPAddress_TextChanged);
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnStart.Location = new System.Drawing.Point(40, 133);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Padding = new System.Windows.Forms.Padding(4);
            this.btnStart.Size = new System.Drawing.Size(140, 50);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnExit.Location = new System.Drawing.Point(410, 133);
            this.btnExit.Margin = new System.Windows.Forms.Padding(2);
            this.btnExit.Name = "btnExit";
            this.btnExit.Padding = new System.Windows.Forms.Padding(4);
            this.btnExit.Size = new System.Drawing.Size(140, 50);
            this.btnExit.TabIndex = 2;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // lbInputTitle
            // 
            this.lbInputTitle.AutoSize = true;
            this.lbInputTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.lbInputTitle.Location = new System.Drawing.Point(40, 15);
            this.lbInputTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbInputTitle.Name = "lbInputTitle";
            this.lbInputTitle.Size = new System.Drawing.Size(180, 31);
            this.lbInputTitle.TabIndex = 3;
            this.lbInputTitle.Text = "输入目标IP地址";
            // 
            // cbAutoStart
            // 
            this.cbAutoStart.AutoSize = true;
            this.cbAutoStart.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.cbAutoStart.Location = new System.Drawing.Point(414, 14);
            this.cbAutoStart.Margin = new System.Windows.Forms.Padding(2);
            this.cbAutoStart.Name = "cbAutoStart";
            this.cbAutoStart.Size = new System.Drawing.Size(136, 35);
            this.cbAutoStart.TabIndex = 4;
            this.cbAutoStart.Text = "开机启动";
            this.cbAutoStart.UseVisualStyleBackColor = true;
            this.cbAutoStart.CheckedChanged += new System.EventHandler(this.CbAutoStart_CheckedChanged);
            // 
            // InfoText
            // 
            this.InfoText.Location = new System.Drawing.Point(40, 203);
            this.InfoText.Multiline = true;
            this.InfoText.Name = "InfoText";
            this.InfoText.ReadOnly = true;
            this.InfoText.Size = new System.Drawing.Size(510, 113);
            this.InfoText.TabIndex = 5;
            this.InfoText.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(578, 344);
            this.Controls.Add(this.InfoText);
            this.Controls.Add(this.cbAutoStart);
            this.Controls.Add(this.lbInputTitle);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.textIPAddress);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(600, 400);
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

        private TextBox textIPAddress;
        private Button btnStart;
        private Button btnExit;
        private Label lbInputTitle;
        private CheckBox cbAutoStart;
        private TextBox InfoText;
    }
}
