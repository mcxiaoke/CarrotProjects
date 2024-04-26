using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Reflection;
using GenshinNotifier;
using Carrot.Common;


namespace Carrot.AutoLock {
    public partial class MainForm : Form {

        private static readonly string NAME = "CarrotLock";
        private static readonly string RE_IP = @"^\d+\.\d+\.\d+\.\d+$";

        private readonly NotifyIcon notifyIcon;
        private readonly ContextMenuStrip contextMenuStrip;

        private string deviceIP = "";

        private readonly ActiveChecker mChecker;


        public MainForm() {
            InitializeComponent();
            mChecker = new ActiveChecker();
            // 初始化NotifyIcon
            notifyIcon = new NotifyIcon {
                Icon = Properties.Resources.carrot_512,
                Text = NAME
            };

            // 初始化ContextMenuStrip
            contextMenuStrip = new ContextMenuStrip();
            // 添加菜单项 - 显示窗口
            ToolStripMenuItem showMenuItem = new("显示窗口", null, ShowWindowMenuItem_Click);
            contextMenuStrip.Items.Add(showMenuItem);
            // 添加分隔线
            contextMenuStrip.Items.Add(new ToolStripSeparator());
            // 添加菜单项 - 退出应用
            ToolStripMenuItem exitMenuItem = new("退出应用", null, ExitMenuItem_Click);
            contextMenuStrip.Items.Add(exitMenuItem);

            // 为NotifyIcon绑定ContextMenuStrip
            notifyIcon.ContextMenuStrip = contextMenuStrip;

            notifyIcon.Click += NotifyIcon_Click;
        }

        private void MainForm_Load(object sender, EventArgs e) {
            Console.WriteLine("MainForm_Load");
            textIPAddress.Text = ActiveChecker.DEFAULT_IP;
            deviceIP = ActiveChecker.DEFAULT_IP;
            UpdateUI();
        }

        private void MainForm_Resize(object sender, EventArgs e) {
            Console.WriteLine($"MainForm_Resize ${this.WindowState}");
            // 判断窗口是否被最小化
            if (this.WindowState == FormWindowState.Minimized) {
                // 隐藏窗体
                this.Hide();
                // 显示状态栏图标
                notifyIcon.Visible = true;
                // 显示状态栏提示
                //notifyIcon.ShowBalloonTip(1000, NAME, "最小化到状态栏", ToolTipIcon.Info);
            } else {
                ShowWindow();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) {
            Console.WriteLine("MainForm_FormClosing Reason:" + e.CloseReason);
            // You can use the CloseReason property to find out why the event is called.
            // When the user clicks on the close button on the window,
            // e.CloseReason is UserClosing,
            // otherwise it is ApplicationExitCall
            if (e.CloseReason == CloseReason.UserClosing) {
                // 取消关闭动作
                e.Cancel = true;
                // 隐藏窗体
                this.WindowState = FormWindowState.Minimized;
                // 显示状态栏图标
                notifyIcon.Visible = true;
                // 显示状态栏提示
                // notifyIcon.ShowBalloonTip(1000, NAME, "最小化到状态栏", ToolTipIcon.Info);
            }

        }


        private void NotifyIcon_Click(object sender, EventArgs e) {
            // 检查鼠标按钮的状态
            if (((MouseEventArgs)e).Button == MouseButtons.Left) {
                ShowWindow();
            } else if (((MouseEventArgs)e).Button == MouseButtons.Right) {
                // 显示ContextMenu
                contextMenuStrip.Show(Cursor.Position);
            }
        }

        private void ShowWindowMenuItem_Click(object sender, EventArgs e) {
            ShowWindow();
        }

        private void ExitMenuItem_Click(object sender, EventArgs e) {
            Console.WriteLine("ExitMenuItem_Click");
            // 退出应用
            mChecker.Stop();
            Application.Exit();
        }

        private void BtnExit_Click(object sender, EventArgs e) {
            Console.WriteLine("BtnExit_Click");
            mChecker.Stop();
            mChecker.callback = null;
            Application.Exit();

        }

        private void BtnStart_Click(object sender, EventArgs e) {
            Console.WriteLine("BtnStart_Click");
            if (mChecker.IsRunning()) {
                mChecker.Stop();
                mChecker.callback = null;
            } else {
                if (!Regex.IsMatch(deviceIP, RE_IP) || !deviceIP.StartsWith("192.168.")) {
                    MessageBox.Show("IP地址格式不正确");
                    return;
                }
                mChecker.callback = OnStatusChanged;
                mChecker.Start();
            }
        }

        public void OnStatusChanged(string result) {
            Logger.Info("OnStatusChanged");
            if (InvokeRequired) {
                Invoke(new MethodInvoker(UpdateUI));
            } else {
                UpdateUI();
            }
        }

        private void UpdateUI() {
            var running = mChecker.IsRunning();
            textIPAddress.Enabled = !running;
            btnStart.Text = running ? "STOP" : "START";
            var textLines = new List<string> {
                running ? "Status:    Running" : "Status:    Stopped",
            };
            if (running) {
                textLines.Add(mChecker.IsDeviceOnline() ? "Device:    Online" : "Device:    Offline");
                textLines.Add($"InActive:    {(int)mChecker.GetInactiveSeconds()}s");
            }
            InfoText.Text = String.Join(Environment.NewLine, textLines);

        }

        private void ShowWindow() {
            // 显示窗口
            notifyIcon.Visible = false;
            this.Show();
            this.Activate();
            UpdateUI();
        }

        private void CbAutoStart_CheckedChanged(object sender, EventArgs e) {
            var cb = sender as CheckBox;
            Console.WriteLine("CbAutoStart_CheckedChanged " + cb!.Checked);
            ShortcutHelper.EnableAutoStart(cb!.Checked);
        }

        private void TextIPAddress_TextChanged(object sender, EventArgs e) {
            var textBox = sender as TextBox;
            deviceIP = textBox!.Text;
            Console.WriteLine("TextIPAddress_TextChanged " + deviceIP);
        }

    }
}
