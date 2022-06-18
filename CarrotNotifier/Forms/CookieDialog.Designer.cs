namespace GenshinNotifier {
    partial class CookieDialog {
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
            this.YesButton = new System.Windows.Forms.Button();
            this.CookieTextGroup = new System.Windows.Forms.GroupBox();
            this.CookieTextBox = new System.Windows.Forms.TextBox();
            this.CookieLabel = new System.Windows.Forms.RichTextBox();
            this.ClearButton = new System.Windows.Forms.Button();
            this.CookieTextGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // YesButton
            // 
            this.YesButton.AutoSize = true;
            this.YesButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.YesButton.Location = new System.Drawing.Point(519, 457);
            this.YesButton.Margin = new System.Windows.Forms.Padding(8);
            this.YesButton.Name = "YesButton";
            this.YesButton.Padding = new System.Windows.Forms.Padding(8);
            this.YesButton.Size = new System.Drawing.Size(120, 50);
            this.YesButton.TabIndex = 2;
            this.YesButton.Text = "保存";
            this.YesButton.UseVisualStyleBackColor = true;
            this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
            // 
            // CookieTextGroup
            // 
            this.CookieTextGroup.AutoSize = true;
            this.CookieTextGroup.Controls.Add(this.CookieTextBox);
            this.CookieTextGroup.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CookieTextGroup.Location = new System.Drawing.Point(10, 10);
            this.CookieTextGroup.Margin = new System.Windows.Forms.Padding(2);
            this.CookieTextGroup.Name = "CookieTextGroup";
            this.CookieTextGroup.Padding = new System.Windows.Forms.Padding(2);
            this.CookieTextGroup.Size = new System.Drawing.Size(641, 266);
            this.CookieTextGroup.TabIndex = 3;
            this.CookieTextGroup.TabStop = false;
            this.CookieTextGroup.Text = "输入米游社Cookie";
            // 
            // CookieTextBox
            // 
            this.CookieTextBox.Location = new System.Drawing.Point(15, 26);
            this.CookieTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.CookieTextBox.Multiline = true;
            this.CookieTextBox.Name = "CookieTextBox";
            this.CookieTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.CookieTextBox.Size = new System.Drawing.Size(619, 212);
            this.CookieTextBox.TabIndex = 0;
            // 
            // CookieLabel
            // 
            this.CookieLabel.BackColor = System.Drawing.SystemColors.Control;
            this.CookieLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CookieLabel.CausesValidation = false;
            this.CookieLabel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CookieLabel.Location = new System.Drawing.Point(13, 284);
            this.CookieLabel.Margin = new System.Windows.Forms.Padding(6);
            this.CookieLabel.Name = "CookieLabel";
            this.CookieLabel.ReadOnly = true;
            this.CookieLabel.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.CookieLabel.Size = new System.Drawing.Size(628, 148);
            this.CookieLabel.TabIndex = 4;
            this.CookieLabel.Text = "";
            this.CookieLabel.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.CookieLabel_LinkClicked);
            // 
            // ClearButton
            // 
            this.ClearButton.AutoSize = true;
            this.ClearButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ClearButton.Location = new System.Drawing.Point(17, 457);
            this.ClearButton.Margin = new System.Windows.Forms.Padding(8);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Padding = new System.Windows.Forms.Padding(8);
            this.ClearButton.Size = new System.Drawing.Size(120, 50);
            this.ClearButton.TabIndex = 5;
            this.ClearButton.Text = "登出";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // CookieDialog
            // 
            this.AcceptButton = this.YesButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(656, 524);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.CookieLabel);
            this.Controls.Add(this.CookieTextGroup);
            this.Controls.Add(this.YesButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CookieDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "添加帐号";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CookieDialog_FormClosed);
            this.Shown += new System.EventHandler(this.CookieDialog_Shown);
            this.CookieTextGroup.ResumeLayout(false);
            this.CookieTextGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button YesButton;
        private System.Windows.Forms.GroupBox CookieTextGroup;
        private System.Windows.Forms.TextBox CookieTextBox;
        private System.Windows.Forms.RichTextBox CookieLabel;
        private System.Windows.Forms.Button ClearButton;
    }
}