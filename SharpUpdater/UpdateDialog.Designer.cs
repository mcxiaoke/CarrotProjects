namespace SharpUpdater {
    partial class UpdateDialog {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateDialog));
            this.BigButton = new System.Windows.Forms.Button();
            this.BigTextBox = new System.Windows.Forms.RichTextBox();
            this.AProgressBar = new System.Windows.Forms.ProgressBar();
            this.LoadingPic = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingPic)).BeginInit();
            this.SuspendLayout();
            // 
            // BigButton
            // 
            this.BigButton.Enabled = false;
            this.BigButton.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BigButton.Location = new System.Drawing.Point(25, 432);
            this.BigButton.Margin = new System.Windows.Forms.Padding(16);
            this.BigButton.Name = "BigButton";
            this.BigButton.Size = new System.Drawing.Size(724, 72);
            this.BigButton.TabIndex = 0;
            this.BigButton.TabStop = false;
            this.BigButton.Text = "检查更新";
            this.BigButton.UseVisualStyleBackColor = true;
            this.BigButton.Click += new System.EventHandler(this.UpdateButton_Click);
            // 
            // BigTextBox
            // 
            this.BigTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.BigTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.BigTextBox.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BigTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            this.BigTextBox.Location = new System.Drawing.Point(25, 25);
            this.BigTextBox.Margin = new System.Windows.Forms.Padding(16);
            this.BigTextBox.Name = "BigTextBox";
            this.BigTextBox.ReadOnly = true;
            this.BigTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.BigTextBox.Size = new System.Drawing.Size(724, 345);
            this.BigTextBox.TabIndex = 2;
            this.BigTextBox.TabStop = false;
            this.BigTextBox.Text = "";
            this.BigTextBox.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.BigTextBox_LinkClicked);
            // 
            // AProgressBar
            // 
            this.AProgressBar.Location = new System.Drawing.Point(25, 389);
            this.AProgressBar.Name = "AProgressBar";
            this.AProgressBar.Size = new System.Drawing.Size(724, 24);
            this.AProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.AProgressBar.TabIndex = 3;
            this.AProgressBar.Visible = false;
            // 
            // LoadingPic
            // 
            this.LoadingPic.Image = ((System.Drawing.Image)(resources.GetObject("LoadingPic.Image")));
            this.LoadingPic.Location = new System.Drawing.Point(339, 254);
            this.LoadingPic.Margin = new System.Windows.Forms.Padding(16);
            this.LoadingPic.Name = "LoadingPic";
            this.LoadingPic.Size = new System.Drawing.Size(100, 100);
            this.LoadingPic.TabIndex = 4;
            this.LoadingPic.TabStop = false;
            this.LoadingPic.Visible = false;
            // 
            // UpdateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(774, 529);
            this.Controls.Add(this.LoadingPic);
            this.Controls.Add(this.AProgressBar);
            this.Controls.Add(this.BigTextBox);
            this.Controls.Add(this.BigButton);
            this.Font = new System.Drawing.Font("微软雅黑", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SharpUpdater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpdateDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.UpdateDialog_FormClosed);
            this.Load += new System.EventHandler(this.UpdateDialog_Load);
            this.Shown += new System.EventHandler(this.UpdateDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.LoadingPic)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button BigButton;
        private System.Windows.Forms.RichTextBox BigTextBox;
        private System.Windows.Forms.ProgressBar AProgressBar;
        private System.Windows.Forms.PictureBox LoadingPic;
    }
}

