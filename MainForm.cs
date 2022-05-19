using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenshinNotifier.Net;

namespace GenshinNotifier {
    public partial class MainForm : Form {

        public MainForm() {
            InitializeComponent();
        }


        private void MainForm_Load(object sender, EventArgs e) {
            var am = AssemblyName.GetAssemblyName("GenshinNotifier.exe");
            this.Text = $"{am.Name} v{am.Version}";
            this.Shown += OnFormShow;
            this.RefreshButton.Click += OnRefershButtonClicked;
            this.OptionButton.Click += OnOptionButtonClicked;
            this.RefreshButton.Enabled = false;
        }

        async void OnFormShow(object sender, EventArgs e) {
            await DataController.Default.Initialize();
            Logger.Info($"OnFormShow {DataController.Default.Cookie}");
            Logger.Info(DataController.Default.Ready ? "Ready" : "NotReady");
            if (DataController.Default.Ready) {
                //await RefreshDailyNote(sender, e);
                this.RefreshButton.Enabled = true;
            } else {
                //AccountValueL.Text = "提示：请进入选项界面设置Cookie后使用";
                //AccountValueL.ForeColor = Color.Red;
                ShowCookieDialog();
            }
        }

        void ShowCookieDialog() {
            var cd = new CookieDialog();
            cd.FormClosed += async (fo, fe) => {
                Logger.Info($"ShowCookieDialog {DataController.Default.CookieChanged}");
                if (DataController.Default.CookieChanged) {
                    await DataController.Default.Initialize();
                    await RefreshDailyNote(null, null);
                }
            };
            cd.Location = new Point(this.Location.X + 160, this.Location.Y + 80);
            cd.ShowDialog();
        }

        async void OnRefershButtonClicked(object sender, EventArgs e) {
            await RefreshDailyNote(sender, e);
        }

        void OnOptionButtonClicked(object sender, EventArgs e) {
        }

        void UpdateRefreshState(bool loading) {
            this.RefreshButton.Enabled = !loading;
            this.RefreshButton.Text = loading ? "更新中..." : "刷新数据";
            this.OptionButton.Enabled = !loading;
            this.LoadingPic.Visible = loading;
        }

        async Task RefreshDailyNote(object sender, EventArgs e) {
            UpdateRefreshState(true);
            var (user, note) = await DataController.Default.GetDailyNote();
            this.RefreshButton.Enabled = true;
            if (user == null || note == null) {
                UpdateRefreshState(false);
                return;
            }
            Logger.Info($"RefreshDailyNote\nuser={user?.GameUid}\nnote={note?.CurrentResin}");
            AccountValueL.Text = $"{user.Nickname} {user.Level}级 / {user.RegionName}({user.Server}) / {user.GameUid}";
            AccountValueL.ForeColor = Color.Blue;


            var resinMayFull = note.CurrentResin >= note.MaxResin - 2;
            ResinValueL.Text = $"{note.CurrentResin}/{note.MaxResin}";
            ResinValueL.ForeColor = resinMayFull ? Color.Red : Color.Green;
            ResinRecValueL.Text = $"{note.ResinRecoveryTimeFormatted}";
            ResinRecValueL.ForeColor = resinMayFull ? Color.Red : Control.DefaultForeColor;
            ResinTimeValueL.Text = $"{note.ResinRecoveryTargetTimeFormatted}";
            ResinTimeValueL.ForeColor = resinMayFull ? Color.Red : Control.DefaultForeColor;

            var expeditionCompleted = note.Expeditions?.All(it => it.RemainedTime == "0") ?? false;
            var expeditionText = $"{note.CurrentExpeditionNum}/{note.MaxExpeditionNum}";
            if (expeditionCompleted) {
                expeditionText += " (已完成)";
            }
            ExpeditionValueL.Text = expeditionText;
            ExpeditionValueL.ForeColor = expeditionCompleted ? Color.Red : Control.DefaultForeColor;

            var taskRewardStr = note.IsExtraTaskRewardReceived ? "(已领取)" : "(未领取)";
            TaskNameValueL.Text = $"{note.FinishedTaskNum}/{note.TotalTaskNum} {taskRewardStr}";
            TaskNameValueL.ForeColor = note.IsExtraTaskRewardReceived ? Control.DefaultForeColor : Color.Red;

            var homeCoinMayFull = note.CurrentHomeCoin >= note.MaxHomeCoin - 100;
            HomeCoinValueL.Text = $"{note.CurrentHomeCoin}/{note.MaxHomeCoin}";
            HomeCoinValueL.ForeColor = homeCoinMayFull ? Color.Red : Control.DefaultForeColor;

            var discountNotUsed = note.ResinDiscountUsedNum < note.ResinDiscountNumLimit;
            DiscountTaskValueL.Text = $"{note.ResinDiscountUsedNum}/{note.ResinDiscountNumLimit}";
            DiscountTaskValueL.ForeColor = discountNotUsed ? Color.Red : Control.DefaultForeColor;

            var transformerReady = note.Transformer.RecoveryTime.Reached;
            TransformerValueL.Text = $"{note.Transformer.RecoveryTime.TimeFormatted}";
            TransformerValueL.ForeColor = (transformerReady ? Color.Red : Control.DefaultForeColor);

            UpdatedValueL.Text = DateTime.Now.ToString("T");
            UpdateRefreshState(false);
        }
    }
}
