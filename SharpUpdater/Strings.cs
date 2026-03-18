using System;

namespace SharpUpdater {

    /// <summary>
    /// UI 文本常量
    /// </summary>
    public static class Strings {

        // ========== 应用信息 ==========
        public const string AppTitle = "SharpUpdater 应用更新工具";
        public const string ProjectUrl = "https://gitee.com/osap/CarrotProjects/tree/master/SharpUpdater";

        // ========== 窗口标题 ==========
        public const string WindowTitle = "{0} Updater";
        public const string TitleNewVersionFound = "发现新版本";
        public const string TitleLatestVersion = "当前已经是最新版";

        // ========== 命令行帮助文本 ==========
        public const string CommandLineUsage = "\n\n使用命令行参数：" +
            "\n-u/--url version-info-url" +
            "\n-n/--name application-name" +
            "\n-c/--config local-config-file" +
            "\n\n使用配置文件: \nSharpUpdater.json" +
            "\n\n 查看帮助: \n" + ProjectUrl;

        // ========== 按钮文本 ==========
        public const string ButtonStartUpdate = "开始更新";
        public const string ButtonExit = "退出";
        public const string ButtonDownloading = "正在下载 ...";
        public const string ButtonVerifying = "正在验证文件 ...";
        public const string ButtonInstalling = "正在安装 ...";
        public const string ButtonRetryDownload = "下载失败，点击重试";
        public const string ButtonRetryVerify = "验证失败，点击重试";
        public const string ButtonRetryInstall = "安装失败，点击重试";
        public const string ButtonUpdateComplete = "更新完成，点击启动";
        public const string ButtonChecking = "检查中 ...";
        public const string ButtonRetryCheck = "点击重试";

        // ========== 对话框标题 ==========
        public const string TitleFileVerifyFailed = "文件校验失败";
        public const string TitleDownloadFailed = "更新包下载失败";
        public const string TitleInstallFailed = "更新包安装失败";
        public const string TitleProcessStopFailed = "无法结束进程";
        public const string TitleConfirmExit = "确认退出";

        // ========== 警告消息 ==========
        public const string WarningUpdateInProgress = "更新正在进行中，确定要退出吗？";
        public const string WarningUpdateCancelled = "更新已取消";

        // ========== 信息标签 ==========
        public const string InfoAppName = "应用名称";
        public const string InfoAppVersion = "应用版本";
        public const string InfoFileSize = "文件大小";
        public const string InfoPublishTime = "发布时间";
        public const string InfoProjectUrl = "项目地址";
        public const string InfoChangelog = "更新说明";

        // ========== 错误消息 ==========
        public const string ErrorStartupFailed = "启动参数错误：";
        public const string ErrorConfigNotFound = "未找到有效的配置文件";
        public const string ErrorConfigMalformed = "配置文件格式错误或 URL 为空";
        public const string ErrorVersionInfoMalformed = "配置信息错误";
        public const string ErrorVersionInfoParseFailed = "无法获取版本更新信息";
        public const string ErrorNoUpdate = "当前已是最新版本";
        public const string ErrorUpdateNotFound = "未找到更新信息";
        public const string ErrorReadLocalVersionFailed = "无法读取本地程序版本信息";

        public static string ErrorDownloadFailed(string url, Exception error) =>
            $"网址：{url}\n\n{error}";

        public static string ErrorFileCorrupted(string filepath) =>
            $"文件可能已损坏或被篡改，请重新下载或联系开发者。\n\n文件：{filepath}";

        public static string ErrorProcessRunning(string program, Exception error) =>
            $"待更新的应用正在运行，请退出后重试\n程序：{program}\n\n{error}";

        public static string ErrorInstallFailed(string filepath, Exception error) =>
            $"文件：{filepath}\n\n{error}";

        public static string ErrorPackageExeNotFound(string program) =>
            $"升级包损坏或错误：可执行文件 [{program}] 不存在";

        public static string ErrorConfigInvalid(string configText) =>
            $"配置错误：配置无效或缺少必须字段！\n\n{configText}";

        public static string ErrorExeNotFound(string program, string appBase, string programAgain) =>
            $"文件错误：可执行文件 [{program}] 不存在！\n\n" +
            $"当前目录 {appBase} 未找到文件名为 {programAgain} 的可执行文件，" +
            $"如果你曾经给文件更名，请改回 {programAgain} 后重试";

        public static string ErrorCheckUpdateFailed(string errorMessage, string url, string exceptionDetails) =>
            $"遇到错误：{errorMessage}\n\n{url}\n{exceptionDetails}";

        // ========== 更新信息 ==========
        public static string UpdateInfoHeader(string name, string currentVersion) =>
            $"【{name}】当前版本：{currentVersion}";

        public static string UpdateInfoAvailable(string latestVersion) =>
            $"\n最新版本：{latestVersion}\n";

        public static string UpdateInfoNoUpdate(string currentVersion) =>
            $"当前版本：{currentVersion} 已是最新版本";
    }
}
