﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Carrot.Common;
using Newtonsoft.Json;
using Semver;

namespace SharpUpdater {

    public enum UpdateStatus {
        NONE,
        READY,
        DONE,
        QUIT,
        ERROR
    }

    public partial class UpdateDialog : Form {
        private SharpConfig myConfig;
        private VersionInfo? updateVersionInfo;
        private UpdateStatus currentUpdateStatus = UpdateStatus.NONE;

        private const string ProjectUrl = "https://gitee.com/osap/CarrotProjects/tree/master/SharpUpdater";

        private HttpClientHandler handler = new HttpClientHandler() { AllowAutoRedirect = true };

        private HttpClient client = new HttpClient() {
            Timeout = TimeSpan.FromSeconds(30),
        };

        public UpdateDialog(CommandOptions options) {
            InitializeComponent();
            var cfg = new SharpConfig(options.Name, options.URL);
            Logger.Debug($"UpdateDialog sc={myConfig}");
            if (cfg.Malformed && !string.IsNullOrWhiteSpace(options.ConfigFile)) {
                cfg = SharpConfig.Read(options.ConfigFile!);
                Logger.Debug($"UpdateDialog config={myConfig}");
            }
            if (cfg?.Malformed == true) {
                cfg = SharpConfig.Read();
                Logger.Debug($"UpdateDialog default={myConfig}");
            }
            if (cfg == null) {
                cfg = new SharpConfig();
            }
            myConfig = cfg;

            Logger.Debug($"UpdateDialog final={myConfig}");
        }

        private async void UpdateDialog_Load(object sender, EventArgs e) {
            if (myConfig?.Malformed != false) {
                SetFatalStatusInfo("启动参数错误：" +
                    $"\n\n使用命令行参数：" +
                    $"\n-u/--url version-info-url" +
                    "\n-n/--name application-name" +
                    "\n-c/--config local-config-file" +
                    "\n\n使用配置文件: \nSharpUpdater.json" +
                    $"\n\n 查看帮助: \n{ProjectUrl}");
            } else {
                await CheckUpdate();
            }
        }

        private void UpdateDialog_FormClosing(object sender, FormClosingEventArgs e) {
        }

        private void UpdateDialog_FormClosed(object sender, FormClosedEventArgs e) {
        }

        private void UpdateDialog_Shown(object sender, EventArgs e) {
            this.Text = $"{myConfig.Name ?? "Sharp"} Updater";
        }

        private static FileVersionInfo? ReadFileVersion(string path) {
            try {
                return FileVersionInfo.GetVersionInfo(path);
            } catch (Exception ex) {
                Logger.Error("ReadFileVersion", ex);
                return null;
            }
        }

        private void SetFatalStatusInfo(string errorText) {
            currentUpdateStatus = UpdateStatus.QUIT;
            Invoke(new Action(() => {
                BigTextBox.Text = errorText;
                BigTextBox.ForeColor = Color.Blue;
                BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 8F);
                BigButton.Enabled = true;
                BigButton.Text = "退出";
            }));
        }

        private void SetRetryStatusInfo(string errorText) {
            currentUpdateStatus = UpdateStatus.ERROR;
            Invoke(new Action(() => {
                BigTextBox.Text = errorText;
                BigTextBox.ForeColor = Color.Blue;
                BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 8F);
                BigButton.Enabled = true;
                BigButton.Text = "点击重试";
            }));
        }

        private void SetVersionInfoTextBox(VersionInfo info) {
            BigTextBox.Font = new System.Drawing.Font(BigTextBox.Font.Name, 9F);
            BigTextBox.ForeColor = Control.DefaultForeColor;
            var infoStr = "";
            infoStr += $"★ 应用名称：{info.Name}\n";
            infoStr += $"★ 应用版本：{info.LocalVersion} => {info.Version}\n";
            infoStr += $"★ 文件大小：{SharpUtils.FormatFileSize(info.DownloadSize)}\n";
            infoStr += $"★ 发布时间：{info.CreatedAt}\n";
            infoStr += $"★ 项目地址：{info.ProjectUrl}\n";
            infoStr += $"★ 更新说明：{info.Changelog}";
            BigTextBox.Text = infoStr;
        }

        private async Task CheckUpdate(string? versionUrl = null) {
            BigTextBox.Text = string.Empty;
            var url = versionUrl ?? myConfig.URL;
            Logger.Debug($"CheckUpdate url={url} ");
            try {
                var text = await client.GetStringAsync(new Uri(url));
                var info = JsonConvert.DeserializeObject<VersionInfo>(text);
                if (info == null) {
                    throw new ArgumentException("无法获取版本更新信息");
                }
                if (VersionInfo.DataInValid(info)) {
                    SetRetryStatusInfo($"配置错误：配置无效或缺少必须字段！\n\n{text}");
                    return;
                }
                updateVersionInfo = info;
                Logger.Debug($"CheckUpdate info={info}");
                var exePath = Path.Combine(SharpConfig.AppBase, info.Program);
                if (!File.Exists(exePath)) {
                    SetRetryStatusInfo($"文件错误：可执行文件 [{info.Program}] 不存在！\n\n" +
                    $"当前目录 {SharpConfig.AppBase} 未找到文件名为 {info.Program} 的可执行文件，" +
                    $"如果你曾经给文件更名，请改回 {info.Program} 后重试");
                    return;
                }
                Logger.Debug($"CheckUpdate exePath={exePath}");
                if (ReadFileVersion(exePath) is FileVersionInfo localFile) {
                    SharpConfig.Write(new SharpConfig(localFile.ProductName!, url));
                    var localVer = SemVersion.Parse(localFile.ProductVersion, SemVersionStyles.Any);
                    var remoteVer = SemVersion.Parse(info.Version, SemVersionStyles.Any);

                    info.LocalName = localFile.ProductName!;
                    info.LocalVersion = localFile.ProductVersion!;

                    bool hasNew = info.HasUpdate && localVer < remoteVer;

                    Logger.Debug($"CheckUpdate end {DateTime.Now}");
                    currentUpdateStatus = hasNew ? UpdateStatus.READY : UpdateStatus.QUIT;
                    Invoke(new Action(() => {
                        this.Text = hasNew ? $"发现新版本" : "当前已经是最新版";
                        SetVersionInfoTextBox(info);
                        BigButton.Enabled = true;
                        BigButton.Text = hasNew ? "开始更新" : "退出";
                    }));
                } else {
                    throw new ArgumentException("无法读取本地程序版本信息");
                }

            } catch (Exception ex) {
                Logger.Debug($"CheckUpdate failed error={ex.Message}");
                SetRetryStatusInfo($"遇到错误：{ex.Message}\n\n{url}\n{ex}");
            }
        }

        private void BigTextBox_LinkClicked(object sender, LinkClickedEventArgs e) {
            Logger.Debug($"InfoTextBox_LinkClicked {e.LinkText}");
            if (e.LinkText is String url) {
                Process.Start(url);
            }

        }

        // http://simplygenius.net/Article/AncillaryAsyncProgress
        // https://devblogs.microsoft.com/dotnet/async-in-4-5-enabling-progress-and-cancellation-in-async-apis/
        private async Task<(string?, Exception?)> DownloadFileAsync(VersionInfo info, IProgress<float> progress) {
            // url for test
            Uri uri = new Uri(info.DownloadUrl);
            Logger.Debug($"DownloadFileAsync url={uri}");
            string filepath = Path.Combine(SharpConfig.AppBase, $"UpdatePackage_{info.Version}.zip");
            //string filepath = Path.GetTempFileName();
            Logger.Debug($"DownloadFileAsync dest={filepath}");
            var cts = new CancellationTokenSource();
            var ctk = cts.Token;
            // make io operations async
            try {
                await Task.Run(() => {
                    if (File.Exists(filepath)) {
                        File.Delete(filepath);
                    }
                });
            } catch (Exception ex) {
                Logger.Debug($"DownloadFileAsync error1={ex}");
                return (null, ex);
            }

            try {
                // Create a file stream to store the downloaded data.
                // This really can be any type of writeable stream.
                using (var file = new FileStream(filepath, FileMode.Create)) {
                    // Use the custom extension method below to download the data.
                    // The passed progress-instance will receive the download status updates.
                    await client.DownloadAsync(uri, file, progress, ctk);
                }
                Logger.Debug($"DownloadFileAsync file={filepath}");
                return (filepath, null);
            } catch (Exception ex) {
                Logger.Debug($"DownloadFileAsync error2={ex}");
                return (null, ex);
            }
        }

        private static async Task<Exception?> InstallUpdateAsync(VersionInfo info, string filepath) {
            // Normalizes the path.
            var program = info.Program;
            var zipPath = Path.GetFullPath(filepath);
            var destPath = Path.GetFullPath(SharpConfig.AppBase);
            Logger.Debug($"InstallUpdateAsync file={filepath}");
            return await Task.Run(() => {
                try {
                    var found = SharpUtils.ZipFileFind(zipPath, program);
                    if (found == null) {
                        throw new NullReferenceException($"升级包损坏或错误：可执行文件 [{program}] 不存在");
                    }
                    bool strip = found.Contains("/") && found.Contains(program);
                    string stripPrefix = found.Replace(program, "");
                    SharpUtils.UnzipFile(zipPath, destPath, true, strip, stripPrefix);
                    //File.Delete(zipPath);
                    return null;
                } catch (Exception ex) {
                    Logger.Debug($"InstallUpdateAsync error={ex.Message}");
                    return ex;
                }
            });
        }

        private static Exception? StopRunningProgram(VersionInfo info) {
            var fullpath = Path.Combine(SharpConfig.AppBase, info.Program);
            Logger.Debug($"StopRunningProgram fullpath={fullpath}");
            return SharpUtils.StopProcessByPath(fullpath);
        }

        private async void UpdateButton_Click(object sender, EventArgs e) {
            if (currentUpdateStatus == UpdateStatus.ERROR) {
                BigTextBox.Text = string.Empty;
                await CheckUpdate();
                return;
            }
            if (currentUpdateStatus == UpdateStatus.DONE) {
                Close();
                if (updateVersionInfo?.Program is string exeName) {
                    Process.Start(Path.Combine(SharpConfig.AppBase, exeName));
                }
                return;
            }
            if (currentUpdateStatus != UpdateStatus.READY) {
                Close();
                return;
            }
            if (updateVersionInfo is null) {
                return;
            }
            var p = new Progress<float>(value => AProgressBar.Value = Convert.ToInt32(value));
            BigButton.Enabled = false;
            BigButton.Text = "正在下载 ...";
            AProgressBar.Visible = true;
            AProgressBar.Value = 0;
            var (filepath, err) = await DownloadFileAsync(updateVersionInfo, p);
            if (err != null) {
                currentUpdateStatus = UpdateStatus.READY;
                BigButton.Enabled = true;
                BigButton.Text = "下载失败，点击重试";
                MessageBox.Show($"网址：{updateVersionInfo.DownloadUrl}\n\n{err}", $"更新包下载失败 {err.GetType()}", MessageBoxButtons.OK);
                return;
            }
            BigButton.Text = "正在安装 ...";
            err = StopRunningProgram(updateVersionInfo);
            if (err != null) {
                currentUpdateStatus = UpdateStatus.READY;
                BigButton.Enabled = true;
                BigButton.Text = "安装失败，点击重试";
                MessageBox.Show($"待更新的应用正在运行，请退出后重试\n程序：{updateVersionInfo.Program}\n\n{err}", $"无法结束进程 {err.GetType()}", MessageBoxButtons.OK);
                return;
            }
            if (filepath is string) {
                err = await InstallUpdateAsync(updateVersionInfo, filepath);
                if (err != null) {
                    currentUpdateStatus = UpdateStatus.READY;
                    BigButton.Enabled = true;
                    BigButton.Text = "安装失败，点击重试";
                    MessageBox.Show($"文件：{filepath}\n\n{err}", $"更新包安装失败 {err.GetType()}", MessageBoxButtons.OK);
                    return;
                }
            }
            currentUpdateStatus = UpdateStatus.DONE;
            BigButton.Enabled = true;
            BigButton.Text = "更新完成，点击启动";
        }
    }
}