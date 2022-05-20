namespace GenshinNotifier {
    partial class MainForm {
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.UpdatedValueL = new System.Windows.Forms.Label();
            this.UpdatedNameL = new System.Windows.Forms.Label();
            this.TransformerValueL = new System.Windows.Forms.Label();
            this.TransformerNameL = new System.Windows.Forms.Label();
            this.DiscountTaskValueL = new System.Windows.Forms.Label();
            this.DiscountTaskNameL = new System.Windows.Forms.Label();
            this.HomeCoinValueL = new System.Windows.Forms.Label();
            this.HomeCoinNameL = new System.Windows.Forms.Label();
            this.TaskNameValueL = new System.Windows.Forms.Label();
            this.TaskNameL = new System.Windows.Forms.Label();
            this.ExpeditionValueL = new System.Windows.Forms.Label();
            this.ExpeditionNameL = new System.Windows.Forms.Label();
            this.ResinTimeValueL = new System.Windows.Forms.Label();
            this.ResinTimeNameL = new System.Windows.Forms.Label();
            this.ResinRecValueL = new System.Windows.Forms.Label();
            this.ResinRecNameL = new System.Windows.Forms.Label();
            this.ResinValueL = new System.Windows.Forms.Label();
            this.ResinNameL = new System.Windows.Forms.Label();
            this.AccountValueL = new System.Windows.Forms.Label();
            this.OptionButton = new System.Windows.Forms.Button();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.CookieButton = new System.Windows.Forms.Button();
            this.LoadingPic = new System.Windows.Forms.PictureBox();
            this.AppNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.NotifyMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemShow = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemCheckin = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemQuit = new System.Windows.Forms.ToolStripMenuItem();
            this.MainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingPic)).BeginInit();
            this.NotifyMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.AutoScroll = true;
            this.MainLayout.AutoSize = true;
            this.MainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.BackColor = System.Drawing.SystemColors.Control;
            this.MainLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainLayout.Controls.Add(this.UpdatedValueL, 1, 9);
            this.MainLayout.Controls.Add(this.UpdatedNameL, 0, 9);
            this.MainLayout.Controls.Add(this.TransformerValueL, 1, 8);
            this.MainLayout.Controls.Add(this.TransformerNameL, 0, 8);
            this.MainLayout.Controls.Add(this.DiscountTaskValueL, 1, 7);
            this.MainLayout.Controls.Add(this.DiscountTaskNameL, 0, 7);
            this.MainLayout.Controls.Add(this.HomeCoinValueL, 1, 6);
            this.MainLayout.Controls.Add(this.HomeCoinNameL, 0, 6);
            this.MainLayout.Controls.Add(this.TaskNameValueL, 1, 5);
            this.MainLayout.Controls.Add(this.TaskNameL, 0, 5);
            this.MainLayout.Controls.Add(this.ExpeditionValueL, 1, 4);
            this.MainLayout.Controls.Add(this.ExpeditionNameL, 0, 4);
            this.MainLayout.Controls.Add(this.ResinTimeValueL, 1, 3);
            this.MainLayout.Controls.Add(this.ResinTimeNameL, 0, 3);
            this.MainLayout.Controls.Add(this.ResinRecValueL, 1, 2);
            this.MainLayout.Controls.Add(this.ResinRecNameL, 0, 2);
            this.MainLayout.Controls.Add(this.ResinValueL, 1, 1);
            this.MainLayout.Controls.Add(this.ResinNameL, 0, 1);
            this.MainLayout.Controls.Add(this.AccountValueL, 0, 0);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainLayout.Location = new System.Drawing.Point(10, 10);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.Padding = new System.Windows.Forms.Padding(2);
            this.MainLayout.RowCount = 10;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainLayout.Size = new System.Drawing.Size(634, 525);
            this.MainLayout.TabIndex = 1;
            // 
            // UpdatedValueL
            // 
            this.UpdatedValueL.AutoSize = true;
            this.UpdatedValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UpdatedValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UpdatedValueL.Location = new System.Drawing.Point(320, 471);
            this.UpdatedValueL.Name = "UpdatedValueL";
            this.UpdatedValueL.Padding = new System.Windows.Forms.Padding(10);
            this.UpdatedValueL.Size = new System.Drawing.Size(308, 51);
            this.UpdatedValueL.TabIndex = 19;
            this.UpdatedValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // UpdatedNameL
            // 
            this.UpdatedNameL.AutoSize = true;
            this.UpdatedNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.UpdatedNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.UpdatedNameL.Location = new System.Drawing.Point(6, 471);
            this.UpdatedNameL.Name = "UpdatedNameL";
            this.UpdatedNameL.Padding = new System.Windows.Forms.Padding(10);
            this.UpdatedNameL.Size = new System.Drawing.Size(307, 51);
            this.UpdatedNameL.TabIndex = 18;
            this.UpdatedNameL.Text = "更新时间";
            this.UpdatedNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TransformerValueL
            // 
            this.TransformerValueL.AutoSize = true;
            this.TransformerValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TransformerValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TransformerValueL.Location = new System.Drawing.Point(320, 419);
            this.TransformerValueL.Name = "TransformerValueL";
            this.TransformerValueL.Padding = new System.Windows.Forms.Padding(10);
            this.TransformerValueL.Size = new System.Drawing.Size(308, 51);
            this.TransformerValueL.TabIndex = 17;
            this.TransformerValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TransformerNameL
            // 
            this.TransformerNameL.AutoSize = true;
            this.TransformerNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TransformerNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TransformerNameL.Location = new System.Drawing.Point(6, 419);
            this.TransformerNameL.Name = "TransformerNameL";
            this.TransformerNameL.Padding = new System.Windows.Forms.Padding(10);
            this.TransformerNameL.Size = new System.Drawing.Size(307, 51);
            this.TransformerNameL.TabIndex = 16;
            this.TransformerNameL.Text = "参量质变仪";
            this.TransformerNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // DiscountTaskValueL
            // 
            this.DiscountTaskValueL.AutoSize = true;
            this.DiscountTaskValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiscountTaskValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.DiscountTaskValueL.Location = new System.Drawing.Point(320, 367);
            this.DiscountTaskValueL.Name = "DiscountTaskValueL";
            this.DiscountTaskValueL.Padding = new System.Windows.Forms.Padding(10);
            this.DiscountTaskValueL.Size = new System.Drawing.Size(308, 51);
            this.DiscountTaskValueL.TabIndex = 15;
            this.DiscountTaskValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DiscountTaskNameL
            // 
            this.DiscountTaskNameL.AutoSize = true;
            this.DiscountTaskNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiscountTaskNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.DiscountTaskNameL.Location = new System.Drawing.Point(6, 367);
            this.DiscountTaskNameL.Name = "DiscountTaskNameL";
            this.DiscountTaskNameL.Padding = new System.Windows.Forms.Padding(10);
            this.DiscountTaskNameL.Size = new System.Drawing.Size(307, 51);
            this.DiscountTaskNameL.TabIndex = 14;
            this.DiscountTaskNameL.Text = "减半周本";
            this.DiscountTaskNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // HomeCoinValueL
            // 
            this.HomeCoinValueL.AutoSize = true;
            this.HomeCoinValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HomeCoinValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.HomeCoinValueL.Location = new System.Drawing.Point(320, 315);
            this.HomeCoinValueL.Name = "HomeCoinValueL";
            this.HomeCoinValueL.Padding = new System.Windows.Forms.Padding(10);
            this.HomeCoinValueL.Size = new System.Drawing.Size(308, 51);
            this.HomeCoinValueL.TabIndex = 13;
            this.HomeCoinValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // HomeCoinNameL
            // 
            this.HomeCoinNameL.AutoSize = true;
            this.HomeCoinNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HomeCoinNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.HomeCoinNameL.Location = new System.Drawing.Point(6, 315);
            this.HomeCoinNameL.Name = "HomeCoinNameL";
            this.HomeCoinNameL.Padding = new System.Windows.Forms.Padding(10);
            this.HomeCoinNameL.Size = new System.Drawing.Size(307, 51);
            this.HomeCoinNameL.TabIndex = 12;
            this.HomeCoinNameL.Text = "洞天宝钱";
            this.HomeCoinNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TaskNameValueL
            // 
            this.TaskNameValueL.AutoSize = true;
            this.TaskNameValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TaskNameValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TaskNameValueL.Location = new System.Drawing.Point(320, 263);
            this.TaskNameValueL.Name = "TaskNameValueL";
            this.TaskNameValueL.Padding = new System.Windows.Forms.Padding(10);
            this.TaskNameValueL.Size = new System.Drawing.Size(308, 51);
            this.TaskNameValueL.TabIndex = 11;
            this.TaskNameValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // TaskNameL
            // 
            this.TaskNameL.AutoSize = true;
            this.TaskNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TaskNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TaskNameL.Location = new System.Drawing.Point(6, 263);
            this.TaskNameL.Name = "TaskNameL";
            this.TaskNameL.Padding = new System.Windows.Forms.Padding(10);
            this.TaskNameL.Size = new System.Drawing.Size(307, 51);
            this.TaskNameL.TabIndex = 10;
            this.TaskNameL.Text = "每日委托";
            this.TaskNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ExpeditionValueL
            // 
            this.ExpeditionValueL.AutoSize = true;
            this.ExpeditionValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExpeditionValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ExpeditionValueL.Location = new System.Drawing.Point(320, 211);
            this.ExpeditionValueL.Name = "ExpeditionValueL";
            this.ExpeditionValueL.Padding = new System.Windows.Forms.Padding(10);
            this.ExpeditionValueL.Size = new System.Drawing.Size(308, 51);
            this.ExpeditionValueL.TabIndex = 9;
            this.ExpeditionValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ExpeditionNameL
            // 
            this.ExpeditionNameL.AutoSize = true;
            this.ExpeditionNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ExpeditionNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ExpeditionNameL.Location = new System.Drawing.Point(6, 211);
            this.ExpeditionNameL.Name = "ExpeditionNameL";
            this.ExpeditionNameL.Padding = new System.Windows.Forms.Padding(10);
            this.ExpeditionNameL.Size = new System.Drawing.Size(307, 51);
            this.ExpeditionNameL.TabIndex = 8;
            this.ExpeditionNameL.Text = "探索派遣";
            this.ExpeditionNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ResinTimeValueL
            // 
            this.ResinTimeValueL.AutoSize = true;
            this.ResinTimeValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResinTimeValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResinTimeValueL.Location = new System.Drawing.Point(320, 159);
            this.ResinTimeValueL.Name = "ResinTimeValueL";
            this.ResinTimeValueL.Padding = new System.Windows.Forms.Padding(10);
            this.ResinTimeValueL.Size = new System.Drawing.Size(308, 51);
            this.ResinTimeValueL.TabIndex = 7;
            this.ResinTimeValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ResinTimeNameL
            // 
            this.ResinTimeNameL.AutoSize = true;
            this.ResinTimeNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResinTimeNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResinTimeNameL.Location = new System.Drawing.Point(6, 159);
            this.ResinTimeNameL.Name = "ResinTimeNameL";
            this.ResinTimeNameL.Padding = new System.Windows.Forms.Padding(10);
            this.ResinTimeNameL.Size = new System.Drawing.Size(307, 51);
            this.ResinTimeNameL.TabIndex = 6;
            this.ResinTimeNameL.Text = "预计恢复时间";
            this.ResinTimeNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ResinRecValueL
            // 
            this.ResinRecValueL.AutoSize = true;
            this.ResinRecValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResinRecValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResinRecValueL.Location = new System.Drawing.Point(320, 107);
            this.ResinRecValueL.Name = "ResinRecValueL";
            this.ResinRecValueL.Padding = new System.Windows.Forms.Padding(10);
            this.ResinRecValueL.Size = new System.Drawing.Size(308, 51);
            this.ResinRecValueL.TabIndex = 5;
            this.ResinRecValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ResinRecNameL
            // 
            this.ResinRecNameL.AutoSize = true;
            this.ResinRecNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResinRecNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResinRecNameL.Location = new System.Drawing.Point(6, 107);
            this.ResinRecNameL.Name = "ResinRecNameL";
            this.ResinRecNameL.Padding = new System.Windows.Forms.Padding(10);
            this.ResinRecNameL.Size = new System.Drawing.Size(307, 51);
            this.ResinRecNameL.TabIndex = 4;
            this.ResinRecNameL.Text = "全部恢复需要";
            this.ResinRecNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ResinValueL
            // 
            this.ResinValueL.AutoSize = true;
            this.ResinValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResinValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResinValueL.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ResinValueL.Location = new System.Drawing.Point(320, 55);
            this.ResinValueL.Name = "ResinValueL";
            this.ResinValueL.Padding = new System.Windows.Forms.Padding(10);
            this.ResinValueL.Size = new System.Drawing.Size(308, 51);
            this.ResinValueL.TabIndex = 3;
            this.ResinValueL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // ResinNameL
            // 
            this.ResinNameL.AutoSize = true;
            this.ResinNameL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResinNameL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ResinNameL.Location = new System.Drawing.Point(6, 55);
            this.ResinNameL.Name = "ResinNameL";
            this.ResinNameL.Padding = new System.Windows.Forms.Padding(10);
            this.ResinNameL.Size = new System.Drawing.Size(307, 51);
            this.ResinNameL.TabIndex = 2;
            this.ResinNameL.Text = "原粹树脂";
            this.ResinNameL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AccountValueL
            // 
            this.AccountValueL.AutoSize = true;
            this.MainLayout.SetColumnSpan(this.AccountValueL, 2);
            this.AccountValueL.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AccountValueL.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.AccountValueL.Location = new System.Drawing.Point(6, 3);
            this.AccountValueL.Name = "AccountValueL";
            this.AccountValueL.Padding = new System.Windows.Forms.Padding(10);
            this.AccountValueL.Size = new System.Drawing.Size(622, 51);
            this.AccountValueL.TabIndex = 0;
            this.AccountValueL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // OptionButton
            // 
            this.OptionButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.OptionButton.Location = new System.Drawing.Point(514, 565);
            this.OptionButton.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.OptionButton.Name = "OptionButton";
            this.OptionButton.Padding = new System.Windows.Forms.Padding(8);
            this.OptionButton.Size = new System.Drawing.Size(130, 61);
            this.OptionButton.TabIndex = 3;
            this.OptionButton.Text = "选项";
            this.OptionButton.UseVisualStyleBackColor = true;
            this.OptionButton.Click += new System.EventHandler(this.OnOptionButtonClicked);
            // 
            // RefreshButton
            // 
            this.RefreshButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RefreshButton.Location = new System.Drawing.Point(232, 565);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Padding = new System.Windows.Forms.Padding(8);
            this.RefreshButton.Size = new System.Drawing.Size(130, 61);
            this.RefreshButton.TabIndex = 4;
            this.RefreshButton.Text = "刷新";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.OnRefershButtonClicked);
            // 
            // CookieButton
            // 
            this.CookieButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CookieButton.Location = new System.Drawing.Point(373, 565);
            this.CookieButton.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.CookieButton.Name = "CookieButton";
            this.CookieButton.Padding = new System.Windows.Forms.Padding(0, 8, 0, 8);
            this.CookieButton.Size = new System.Drawing.Size(130, 61);
            this.CookieButton.TabIndex = 6;
            this.CookieButton.Text = "Cookie";
            this.CookieButton.UseVisualStyleBackColor = true;
            this.CookieButton.Click += new System.EventHandler(this.OnCookieButtonClicked);
            // 
            // LoadingPic
            // 
            this.LoadingPic.Image = global::GenshinNotifier.Properties.Resources.loading_100;
            this.LoadingPic.Location = new System.Drawing.Point(13, 541);
            this.LoadingPic.Name = "LoadingPic";
            this.LoadingPic.Size = new System.Drawing.Size(100, 100);
            this.LoadingPic.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.LoadingPic.TabIndex = 5;
            this.LoadingPic.TabStop = false;
            this.LoadingPic.Visible = false;
            // 
            // AppNotifyIcon
            // 
            this.AppNotifyIcon.ContextMenuStrip = this.NotifyMenuStrip;
            this.AppNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("AppNotifyIcon.Icon")));
            this.AppNotifyIcon.Text = "GenshinNotifier";
            this.AppNotifyIcon.DoubleClick += new System.EventHandler(this.AppNotifyIcon_DoubleClick);
            // 
            // NotifyMenuStrip
            // 
            this.NotifyMenuStrip.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.NotifyMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemShow,
            this.MenuItemCheckin,
            this.MenuItemQuit});
            this.NotifyMenuStrip.Name = "contextMenuStrip1";
            this.NotifyMenuStrip.Size = new System.Drawing.Size(209, 118);
            // 
            // MenuItemShow
            // 
            this.MenuItemShow.Name = "MenuItemShow";
            this.MenuItemShow.Size = new System.Drawing.Size(208, 38);
            this.MenuItemShow.Text = "显示主界面";
            this.MenuItemShow.Click += new System.EventHandler(this.MenuItemShow_Click);
            // 
            // MenuItemCheckin
            // 
            this.MenuItemCheckin.Name = "MenuItemCheckin";
            this.MenuItemCheckin.Size = new System.Drawing.Size(208, 38);
            this.MenuItemCheckin.Text = "米游社签到";
            this.MenuItemCheckin.Click += new System.EventHandler(this.MenuItemCheckin_Click);
            // 
            // MenuItemQuit
            // 
            this.MenuItemQuit.Name = "MenuItemQuit";
            this.MenuItemQuit.Size = new System.Drawing.Size(208, 38);
            this.MenuItemQuit.Text = "退出";
            this.MenuItemQuit.Click += new System.EventHandler(this.MenuItemQuit_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(654, 649);
            this.Controls.Add(this.CookieButton);
            this.Controls.Add(this.LoadingPic);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.OptionButton);
            this.Controls.Add(this.MainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Load += new System.EventHandler(this.OnFormLoad);
            this.Shown += new System.EventHandler(this.OnFormShow);
            this.SizeChanged += new System.EventHandler(this.OnSizeChanged);
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingPic)).EndInit();
            this.NotifyMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.Label AccountValueL;
        private System.Windows.Forms.Label ResinValueL;
        private System.Windows.Forms.Label ResinNameL;
        private System.Windows.Forms.Label UpdatedValueL;
        private System.Windows.Forms.Label UpdatedNameL;
        private System.Windows.Forms.Label TransformerValueL;
        private System.Windows.Forms.Label TransformerNameL;
        private System.Windows.Forms.Label DiscountTaskValueL;
        private System.Windows.Forms.Label DiscountTaskNameL;
        private System.Windows.Forms.Label HomeCoinValueL;
        private System.Windows.Forms.Label HomeCoinNameL;
        private System.Windows.Forms.Label TaskNameValueL;
        private System.Windows.Forms.Label TaskNameL;
        private System.Windows.Forms.Label ExpeditionValueL;
        private System.Windows.Forms.Label ExpeditionNameL;
        private System.Windows.Forms.Label ResinTimeValueL;
        private System.Windows.Forms.Label ResinTimeNameL;
        private System.Windows.Forms.Label ResinRecValueL;
        private System.Windows.Forms.Label ResinRecNameL;
        private System.Windows.Forms.Button OptionButton;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.PictureBox LoadingPic;
        private System.Windows.Forms.Button CookieButton;
        private System.Windows.Forms.NotifyIcon AppNotifyIcon;
        private System.Windows.Forms.ContextMenuStrip NotifyMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem MenuItemShow;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCheckin;
        private System.Windows.Forms.ToolStripMenuItem MenuItemQuit;
    }
}

