using Carrot.Common;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SharpUpdater {

    public enum UpdateStatus {
        NONE,
        READY,
        DONE,
        QUIT,
        ERROR
    }

    public partial class UpdateDialog : Form {
        private readonly SharpConfig _config;
        private readonly UpdateService _updateService;
        private VersionInfo? _updateVersionInfo;
        private UpdateStatus _currentUpdateStatus = UpdateStatus.NONE;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isUpdating;

        private readonly HttpClient _httpClient = new HttpClient() {
            Timeout = TimeSpan.FromSeconds(30),
        };

        public UpdateDialog(CommandOptions options) {
            InitializeComponent();

            // 加载配置
            _config = LoadConfig(options);
            Logger.Debug($"UpdateDialog config={_config}");

            // 初始化更新服务
            _updateService = new UpdateService(_httpClient);
        }

        private static SharpConfig LoadConfig(CommandOptions options) {
            var cfg = new SharpConfig(options.Name, options.URL);

            if (cfg.Malformed && !string.IsNullOrWhiteSpace(options.ConfigFile)) {
                cfg = SharpConfig.Read(options.ConfigFile!);
            }

            if (cfg?.Malformed == true) {
                cfg = SharpConfig.Read();
            }

            return cfg ?? new SharpConfig();
        }

        private async void UpdateDialog_Load(object sender, EventArgs e) {
            if (_config?.Malformed != false) {
                SetFatalStatusInfo(Strings.ErrorStartupFailed + Strings.CommandLineUsage);
            } else {
                await CheckUpdate();
            }
        }

        private void UpdateDialog_Shown(object sender, EventArgs e) {
            this.Text = string.Format(Strings.WindowTitle, _config.Name ?? "Sharp");
        }

        private void UpdateDialog_FormClosing(object sender, FormClosingEventArgs e) {
            // 如果正在更新，提示用户
            if (_isUpdating) {
                var result = MessageBox.Show(
                    Strings.WarningUpdateInProgress,
                    Strings.TitleConfirmExit,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.No) {
                    e.Cancel = true;
                    return;
                }
                // 取消正在进行的操作
                _cancellationTokenSource?.Cancel();
            }
        }

        private void UpdateDialog_FormClosed(object sender, FormClosedEventArgs e) {
            // 释放资源
            _cancellationTokenSource?.Dispose();
            _httpClient.Dispose();
        }

        private void SetFatalStatusInfo(string errorText) {
            _currentUpdateStatus = UpdateStatus.QUIT;
            Invoke(new Action(() => {
                BigTextBox.Text = errorText;
                BigTextBox.ForeColor = Color.Blue;
                BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 8F);
                BigButton.Enabled = true;
                BigButton.Text = Strings.ButtonExit;
            }));
        }

        private void SetRetryStatusInfo(string errorText) {
            _currentUpdateStatus = UpdateStatus.ERROR;
            Invoke(new Action(() => {
                BigTextBox.Text = errorText;
                BigTextBox.ForeColor = Color.Blue;
                BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 8F);
                BigButton.Enabled = true;
                BigButton.Text = Strings.ButtonRetryCheck;
            }));
        }

        private void SetVersionInfoTextBox(VersionInfo info) {
            BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 9F);
            BigTextBox.ForeColor = Control.DefaultForeColor;
            var infoStr = "";
            infoStr += $"★ {Strings.InfoAppName}：{info.Name}\n";
            infoStr += $"★ {Strings.InfoAppVersion}：{info.LocalVersion} => {info.Version}\n";
            infoStr += $"★ {Strings.InfoFileSize}：{SharpUtils.FormatFileSize(info.DownloadSize)}\n";
            infoStr += $"★ {Strings.InfoPublishTime}：{info.CreatedAt}\n";
            infoStr += $"★ {Strings.InfoProjectUrl}：{info.ProjectUrl}\n";
            infoStr += $"★ {Strings.InfoChangelog}：{info.Changelog}";
            BigTextBox.Text = infoStr;
        }

        private async Task CheckUpdate() {
            BigTextBox.Text = string.Empty;
            var result = await _updateService.CheckUpdateAsync(_config.URL);

            if (!result.IsSuccess) {
                SetRetryStatusInfo(result.ErrorMessage!);
                return;
            }

            _updateVersionInfo = result.VersionInfo;

            _currentUpdateStatus = result.HasUpdate ? UpdateStatus.READY : UpdateStatus.QUIT;
            Invoke(new Action(() => {
                this.Text = result.HasUpdate ? Strings.TitleNewVersionFound : Strings.TitleLatestVersion;
                SetVersionInfoTextBox(result.VersionInfo!);
                BigButton.Enabled = true;
                BigButton.Text = result.HasUpdate ? Strings.ButtonStartUpdate : Strings.ButtonExit;
            }));
        }

        private void BigTextBox_LinkClicked(object sender, LinkClickedEventArgs e) {
            Logger.Debug($"InfoTextBox_LinkClicked {e.LinkText}");
            if (e.LinkText is string url) {
                Process.Start(url);
            }
        }

        private async void UpdateButton_Click(object sender, EventArgs e) {
            if (_currentUpdateStatus == UpdateStatus.ERROR) {
                BigTextBox.Text = string.Empty;
                await CheckUpdate();
                return;
            }

            if (_currentUpdateStatus == UpdateStatus.DONE) {
                Close();
                if (_updateVersionInfo?.Program is string exeName) {
                    Process.Start(Path.Combine(SharpConfig.AppBase, exeName));
                }
                return;
            }

            if (_currentUpdateStatus != UpdateStatus.READY || _updateVersionInfo == null) {
                Close();
                return;
            }

            // 开始更新流程
            await PerformUpdateAsync();
        }

        private async Task PerformUpdateAsync() {
            _isUpdating = true;
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var progress = new Progress<float>(value => {
                if (AProgressBar.InvokeRequired) {
                    AProgressBar.Invoke(() => AProgressBar.Value = Convert.ToInt32(value));
                } else {
                    AProgressBar.Value = Convert.ToInt32(value);
                }
            });

            BigButton.Enabled = false;
            AProgressBar.Visible = true;
            AProgressBar.Value = 0;

            string? downloadedFilePath = null;

            try {
                // 步骤1：下载
                BigButton.Text = Strings.ButtonDownloading;
                var downloadResult = await _updateService.DownloadPackageAsync(_updateVersionInfo!, progress, cancellationToken);

                if (!downloadResult.IsSuccess) {
                    if (cancellationToken.IsCancellationRequested) {
                        ShowErrorAndReset(Strings.ButtonStartUpdate, Strings.WarningUpdateCancelled, Strings.TitleConfirmExit);
                    } else {
                        ShowErrorAndReset(Strings.ButtonRetryDownload,
                            Strings.ErrorDownloadFailed(_updateVersionInfo!.DownloadUrl, downloadResult.Error!),
                            Strings.TitleDownloadFailed);
                    }
                    return;
                }

                downloadedFilePath = downloadResult.FilePath;

                // 步骤2：验证文件
                if (!string.IsNullOrWhiteSpace(_updateVersionInfo!.Sha256sum)) {
                    BigButton.Text = Strings.ButtonVerifying;
                    var verifyResult = await Task.Run(() =>
                        UpdateService.VerifyFileHash(downloadResult.FilePath!, _updateVersionInfo.Sha256sum), cancellationToken);

                    if (!verifyResult.IsSuccess && !verifyResult.IsSkipped) {
                        ShowErrorAndReset(Strings.ButtonRetryVerify,
                            verifyResult.ErrorMessage!,
                            Strings.TitleFileVerifyFailed);

                        // 删除损坏的文件
                        try { if (downloadResult.FilePath != null) File.Delete(downloadResult.FilePath); } catch { }
                        return;
                    }
                }

                // 步骤3：停止进程
                BigButton.Text = Strings.ButtonInstalling;
                var stopResult = UpdateService.StopRunningProcess(_updateVersionInfo);

                if (!stopResult.IsSuccess) {
                    ShowErrorAndReset(Strings.ButtonRetryInstall,
                        Strings.ErrorProcessRunning(_updateVersionInfo.Program, stopResult.Error!),
                        Strings.TitleProcessStopFailed);
                    return;
                }

                // 步骤4：安装更新
                var installResult = await UpdateService.InstallUpdateAsync(_updateVersionInfo, downloadResult.FilePath!);

                if (!installResult.IsSuccess) {
                    ShowErrorAndReset(Strings.ButtonRetryInstall,
                        Strings.ErrorInstallFailed(downloadResult.FilePath!, installResult.Error!),
                        Strings.TitleInstallFailed);
                    return;
                }

                // 更新成功，清理下载文件
                try {
                    if (downloadedFilePath != null && File.Exists(downloadedFilePath)) {
                        File.Delete(downloadedFilePath);
                        UpdateLogger.LogInfo("已清理下载文件: {0}", downloadedFilePath);
                    }
                } catch (Exception ex) {
                    UpdateLogger.LogWarning("清理下载文件失败: {0}", ex.Message);
                }

                // 更新成功
                _currentUpdateStatus = UpdateStatus.DONE;
                BigButton.Enabled = true;
                BigButton.Text = Strings.ButtonUpdateComplete;
            } finally {
                _isUpdating = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void ShowErrorAndReset(string buttonText, string errorMessage, string title) {
            _currentUpdateStatus = UpdateStatus.READY;
            BigButton.Enabled = true;
            BigButton.Text = buttonText;
            MessageBox.Show(errorMessage, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
