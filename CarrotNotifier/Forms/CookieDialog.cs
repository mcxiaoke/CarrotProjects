﻿using System;
using System.Windows.Forms;
using Carrot.Common;
using GenshinLib;

namespace GenshinNotifier {

    public partial class CookieDialog : Form {
        public string? OldCookie;
        public string? NewCookie;
        public UserGameRole? NewUser;

        private static string COOKIE_GUIDE =
            "Cookie获取方法：浏览器隐身模式登录 http://bbs.mihoyo.com/ys/ 再登录 https://user.mihoyo.com/ 开开发者工具控制台输入 document.cookie 复制文本。 https://gitee.com/osap/CarrotProjects/issues/I59RIY 这里有详细的说明和动图演示。";

        public event EventHandler<SimpleEventArgs>? CookieHandlers;

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
                    CookieHandlers?.Invoke(this, new SimpleEventArgs(0));
                    DialogResult = DialogResult.Cancel;
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
            if (string.IsNullOrEmpty(tempCookie)) {
                ShowToolTip("Cookie不能为空");
                return;
            }
            if (!Utility.ValiteCookieFields(tempCookie)) {
                ShowToolTip("Cookie无效：缺少必须字段");
                return;
            }
            if (tempCookie == OldCookie) {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            var result = await DataController.ValidateCookie(tempCookie!);
            if (result is UserGameRole user) {
                Logger.Debug($"YesButton_Click uid={user.GameUid}");
                if (user.GameUid != null) {
                    CookieHandlers?.Invoke(this, new SimpleEventArgs(tempCookie!));
                    NewCookie = tempCookie;
                    NewUser = user;
                    DialogResult = DialogResult.OK;
                    Close();
                } else {
                    ShowToolTip("Cookie验证失败，请检查");
                }
            }
        }

        private void CookieLabel_LinkClicked(object sender, LinkClickedEventArgs e) {
            Logger.Debug($"CookieLabel_LinkClicked {e.LinkText}");
            if (e.LinkText is string url) {
                System.Diagnostics.Process.Start(url);
            }
        }

        private void CookieDialog_FormClosed(object sender, FormClosedEventArgs e) {
            Logger.Debug("CookieDialog_FormClosed");
            var handlers = CookieHandlers?.GetInvocationList() ?? new Delegate[] { };
            foreach (EventHandler<SimpleEventArgs> d in handlers) {
                Logger.Debug("CookieDialog_FormClosed -Handlers");
                CookieHandlers -= d;
            }
        }
    }
}