using System;
using System.Windows.Forms;
using CarrotCommon;

namespace GenshinNotifier {

    public partial class CookieDialog : Form {
        private string OldCookie;

        private static string COOKIE_GUIDE =
            @"Cookie获取方法：浏览器隐身模式登录 http://bbs.mihoyo.com/ys/ 再登录 https://user.mihoyo.com/ 开开发者工具控制台输入 document.cookie 复制文本。 https://gitee.com/osap/CarrotProjects/issues/I59RIY 这里有详细的说明和动图演示。";

        public event EventHandler Handlers;

        public CookieDialog() {
            InitializeComponent();
            OldCookie = DataController.Default.Cookie;
            Logger.Debug("CookieDialog.InitializeComponent");
        }

        private void ShowToolTip(string message, int duration = 3000) {
            new ToolTip().Show(message, CookieTextBox, 240, 120, duration);
        }

        private void CookieDialog_Shown(object sender, EventArgs e) {
            Logger.Debug("CookieDialog_Shown");
            // 必备字段只有两个 cookie_token 和 account_id
            this.CookieTextBox.Text = OldCookie;
            this.CookieLabel.Text = COOKIE_GUIDE;
            this.ClearButton.Enabled = !string.IsNullOrWhiteSpace(OldCookie);
            var user = DataController.Default.UserCached;
            if (user != null) {
                this.Text = $"当前帐号：{user.Nickname} / {user.RegionName}({user.Server}) / {user.GameUid}";
            }
        }

        private void ClearButton_Click(object sender, EventArgs e) {
            var cd = new ConfirmDialog("登出确认", "清空Cookie", "我再想想");
            var ret = cd.ShowDialog();
            switch (ret) {
                case DialogResult.No:
                    // clear button
                    Handlers?.Invoke(this, new SimpleEventArgs(null));
                    Close();
                    break;

                case DialogResult.Yes:
                // think button
                case DialogResult.Cancel:
                default:
                    break;
            }
        }

        private async void YesButton_Click(object sender, EventArgs e) {
            var tempCookie = CookieTextBox.Text?.Trim().Replace("\"", "").Replace("'", "");
            Logger.Debug($"YesButton_Click tempCookie: {tempCookie}");
            if (String.IsNullOrEmpty(tempCookie)) {
                ShowToolTip("Cookie不能为空");
                return;
            }
            if (!Utility.ValiteCookieFields(tempCookie)) {
                ShowToolTip("Cookie无效：缺少必须字段");
                return;
            }
            if (tempCookie == OldCookie) {
                Close();
                return;
            }
            var user = await DataController.ValidateCookie(tempCookie);
            Logger.Debug($"YesButton_Click uid={user?.GameUid}");
            if (user?.GameUid != null) {
                Handlers?.Invoke(this, new SimpleEventArgs(tempCookie));
                Close();
            } else {
                ShowToolTip("Cookie验证失败，请检查");
            }
        }

        private void CookieLabel_LinkClicked(object sender, LinkClickedEventArgs e) {
            Logger.Debug($"CookieLabel_LinkClicked {e.LinkText}");
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void CookieDialog_FormClosed(object sender, FormClosedEventArgs e) {
            Logger.Debug("CookieDialog_FormClosed");
            var handlers = Handlers?.GetInvocationList() ?? new Delegate[] { };
            foreach (EventHandler d in handlers) {
                Logger.Debug("CookieDialog_FormClosed -Handlers");
                Handlers -= d;
            }
        }
    }
}