namespace GenshinNotifier {
    partial class ConfirmDialog {
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
            this.ButtonNo = new System.Windows.Forms.Button();
            this.ButtonYes = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonNo
            // 
            this.ButtonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.ButtonNo.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ButtonNo.Location = new System.Drawing.Point(86, 21);
            this.ButtonNo.Margin = new System.Windows.Forms.Padding(12);
            this.ButtonNo.Name = "ButtonNo";
            this.ButtonNo.Size = new System.Drawing.Size(400, 70);
            this.ButtonNo.TabIndex = 0;
            this.ButtonNo.Text = "ButtonNo";
            this.ButtonNo.UseVisualStyleBackColor = true;
            this.ButtonNo.Click += new System.EventHandler(this.ButtonA_Click);
            // 
            // ButtonYes
            // 
            this.ButtonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.ButtonYes.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ButtonYes.Location = new System.Drawing.Point(86, 115);
            this.ButtonYes.Margin = new System.Windows.Forms.Padding(12);
            this.ButtonYes.Name = "ButtonYes";
            this.ButtonYes.Size = new System.Drawing.Size(400, 70);
            this.ButtonYes.TabIndex = 1;
            this.ButtonYes.Text = "ButtonYes";
            this.ButtonYes.UseVisualStyleBackColor = true;
            this.ButtonYes.Click += new System.EventHandler(this.ButtonB_Click);
            // 
            // ConfirmDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(574, 229);
            this.Controls.Add(this.ButtonYes);
            this.Controls.Add(this.ButtonNo);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfirmDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Title";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ConfirmDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ButtonNo;
        private System.Windows.Forms.Button ButtonYes;
    }
}