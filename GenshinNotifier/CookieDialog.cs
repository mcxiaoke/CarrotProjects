using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CarrotCommon;
using GenshinLib;

namespace GenshinNotifier {
    public partial class CookieDialog : Form {
        private string OldCookie;

        private static string COOKIE_GUIDE =
            @"说明：
浏览器隐身模式打开 http://bbs.mihoyo.com/ys/ 登录，新标签页打开 https://user.mihoyo.com/ 再次登录，按下F12打开开发者工具，点击控制台，输入 document.cookie 按回车，复制出现的字符串，粘贴到输入框，点击保存。 https://gitee.com/osap/CarrotProjects/issues/I59RIY 这里有详细说明。";

        public event EventHandler Handlers;

        public CookieDialog() {
            InitializeComponent();
            OldCookie = DataController.Default.Cookie;
            this.CookieTextBox.Text = OldCookie;
            this.CookieLabel.Text = COOKIE_GUIDE;
            Logger.Debug("CookieDialog.InitializeComponent");
        }

        private void ShowToolTip(string message, int duration = 3000) {
            new ToolTip().Show(message, CookieTextBox, 240, 120, duration);
        }

        private void CookieDialog_Shown(object sender, EventArgs e) {
            Logger.Debug("CookieDialog_Shown");
            // 必备字段只有两个 cookie_token 和 account_id
        }

        private void NoButton_Click(object sender, EventArgs e) {
            Close();
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
