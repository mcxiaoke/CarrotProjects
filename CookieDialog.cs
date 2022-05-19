using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinNotifier.Net;

namespace GenshinNotifier {
    public partial class CookieDialog : Form {
        private string savedCookie;

        private static string COOKIE_GUIDE = 
            @"使用说明：
1. 打开 http://bbs.mihoyo.com/ys/ 并进行登录操作；
2. 新建标签页，打开 http://user.mihoyo.com/ 并进行登录操作；
3. 按下键盘上的F12或右键检查,打开开发者工具,点击控制台；
4. 控制台输入 document.cookie 复制出现的字符串；
5. 将复制好的Cookie字符串粘贴到输入框，点击保存";

        public CookieDialog() {
            InitializeComponent();
            savedCookie = DataController.Default.Cookie;
            this.CookieTextBox.Text = savedCookie;
            this.CookieLabel.Text = COOKIE_GUIDE;
        }

        private void showToolTip(string message, int duration = 3000) {
            new ToolTip().Show(message, CookieTextBox, 240, 120, duration);
        }

        private void CookieDialog_Shown(object sender, EventArgs e) {
            Logger.Debug("CookieDialog_Shown");
        }

        private void NoButton_Click(object sender, EventArgs e) {
            Close();
        }

        private async void YesButton_Click(object sender, EventArgs e) {
            var tempCookie = CookieTextBox.Text?.Trim().Replace("\"", "").Replace("'", "");
            Logger.Info(tempCookie);
            if (String.IsNullOrEmpty(tempCookie)) {
                showToolTip("Cookie不能为空");
            } else {
                var cookieDict = Utility.ParseCookieString(tempCookie);
                if (!cookieDict.ContainsKey("login_ticket")) {
                    showToolTip("Cookie无效：不包含login_ticket字段");
                } else {
                    var user = await DataController.ValidateCookie(tempCookie);
                    Logger.Info($"CookieValidateButton_Click {user}");
                    showToolTip(user?.GameUid != null ? "Cookie有效，已保存" : "Cookie无效，请检查");
                    if (user?.GameUid != null) {
                        // save cookie to settings
                        DataController.Default.Cookie = tempCookie;
                        Properties.Settings.Default.MihoyoCookie = tempCookie;
                        Properties.Settings.Default.Save();
                        DataController.Default.Cookie = tempCookie;
                        Close();
                    }
                }

            }
        }

        private void CookieLabel_LinkClicked(object sender, LinkClickedEventArgs e) {
            Logger.Debug($"CookieLabel_LinkClicked {e.LinkText}");
            System.Diagnostics.Process.Start(e.LinkText);
        }

    }
}
