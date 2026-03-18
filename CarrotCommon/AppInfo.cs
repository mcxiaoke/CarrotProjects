using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Resources;

namespace Carrot.Common;

/// <summary>
/// Provides information about the current application.
/// 提供有关当前应用程序的信息。
/// </summary>
public static class AppInfo {
    private static readonly Lazy<ApplicationData> _lazy = new(() => new ApplicationData());
    private static ApplicationData AppData => _lazy.Value;

    /// <summary>
    /// Gets the assembly name of the application (without extension).
    /// 获取应用程序的程序集文件的名称（不含扩展名）。
    /// </summary>
    public static string AssemblyName => AppData.AssemblyName;

    /// <summary>
    /// Gets the executable file name, including the extension.
    /// 可执行文件文件名，包含文件名和扩展名。
    /// </summary>
    public static string ModuleName => AppData.ModuleName;

    /// <summary>
    /// Gets the full path of the executable, including the file name.
    /// 可执行文件的路径，包括可执行文件的名称。
    /// </summary>
    public static string ExecutablePath => AppData.ExecutablePath;

    /// <summary>
    /// Gets the path of the executable, excluding the file name.
    /// 可执行文件的路径，不包括可执行文件的名称。
    /// </summary>
    public static string StartupPath => AppData.StartupPath;

    /// <summary>
    /// Gets the company name associated with the application.
    /// 获取与该应用程序关联的公司名称。
    /// </summary>
    public static string CompanyName => AppData.CompanyName;

    /// <summary>
    /// Gets the product name associated with the application.
    /// 获取与应用程序关联的产品名称。
    /// </summary>
    public static string ProductName => AppData.ProductName;

    /// <summary>
    /// Gets the product version associated with the application.
    /// 获取与该应用程序关联的产品版本。
    /// </summary>
    public static string ProductVersion => AppData.ProductVersion;

    /// <summary>
    /// Gets the file version.
    /// 获取文件版本号。
    /// </summary>
    public static string FileVersion => AppData.FileVersion;

    /// <summary>
    /// Gets the title associated with the application.
    /// 获取与应用程序关联的标题。
    /// </summary>
    public static string Title => AppData.Title;

    /// <summary>
    /// Gets the copyright notice associated with the application.
    /// 获取与应用程序关联的版权声明。
    /// </summary>
    public static string Copyright => AppData.Copyright;

    /// <summary>
    /// Gets the description associated with the application.
    /// 获取与应用程序关联的说明。
    /// </summary>
    public static string Description => AppData.Description;

    /// <summary>
    /// Gets the culture information of the current thread.
    /// 获取或设置当前线程的区域性信息。
    /// </summary>
    public static string CurrentCulture => AppData.CurrentCulture;

    /// <summary>
    /// Gets the FileVersionInfo object.
    /// 获取 FileVersionInfo 对象。
    /// </summary>
    public static FileVersionInfo FileInfo => AppData.FileInfo;

    public static string AsString() {
        return AppData.ToString();
    }

    /// <summary>
    /// Gets the path for application data shared by all users.
    /// 获取所有用户共享的应用程序数据的路径。
    /// </summary>
    public static string CommonAppDataPath
        => GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

    /// <summary>
    /// Gets the path for the local, non-roaming user's application data.
    /// 获取本地、非漫游用户的应用程序数据的路径。
    /// </summary>
    public static string LocalAppDataPath => GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    /// <summary>
    /// Gets the path for the user's roaming application data.
    /// 获取用户的应用程序数据的路径。
    /// </summary>
    public static string RoamingAppDataPath => GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

    /// <summary>
    /// Gets the registry key path for application data.
    /// 获取应用程序数据的注册表项。
    /// </summary>
    public static string AppDataRegistryPath => $"Software\\{CompanyName}\\{ProductName}";

    public static string UserStartupFolder => Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    public static string UserDesktopFolder => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    /// <summary>
    /// Constructs a data path: basePath + CompanyName + ProductName [+ ProductVersion].
    /// Creates the directory if it doesn't exist.
    /// 构建数据路径：basePath + CompanyName + ProductName [+ ProductVersion].
    /// 如果目录不存在，也将创建该目录。
    /// </summary>
    internal static string GetDataPath(string basePath, bool containsVersion = false) {
        string path = Path.Combine(basePath, CompanyName, ProductName);
        if (containsVersion) { path = Path.Combine(path, ProductVersion); }
        Storage.CheckOrCreateDir(path);
        return path;
    }
}

internal class ApplicationData {

    // from System.Windows.Forms.Application
    // from Microsoft.VisualBasic.ApplicationServices.AssemblyInfo
    // from System.Diagnostics.FileVersionInfo
    public string AssemblyName { get; private set; } = string.Empty;

    public string ModuleName { get; private set; } = string.Empty;
    public string ExecutablePath { get; private set; } = string.Empty;
    public string StartupPath { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string ProductVersion { get; private set; } = string.Empty;
    public string FileVersion { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Copyright { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string CurrentCulture { get; private set; } = string.Empty;

    [JsonIgnore]
    public FileVersionInfo FileInfo { get; private set; } = default!;

    public ApplicationData() {
        var process = Process.GetCurrentProcess();
        var module = process.MainModule;

        if (module != null) {
            ModuleName = module.ModuleName ?? string.Empty;
            FileInfo = module.FileVersionInfo;
            // https://stackoverflow.com/questions/64581054
            ExecutablePath = module.FileName ?? string.Empty;
        } else {
            // Fallback or handle null module if necessary (e.g. specialized environments)
            FileInfo = FileVersionInfo.GetVersionInfo(process.MainModule?.FileName ?? Environment.ProcessPath ?? string.Empty);
        }

        StartupPath = AppContext.BaseDirectory;

        Assembly entryAssembly = Assembly.GetEntryAssembly()!;
        if (entryAssembly != null) {
            var mainType = entryAssembly.EntryPoint?.ReflectedType;
            AssemblyName = entryAssembly.GetName().Name ?? ModuleName;
            ParseAssembly(entryAssembly);

            // Fallbacks
            CompanyName = string.IsNullOrEmpty(CompanyName) ? (FileInfo?.CompanyName ?? mainType?.Namespace ?? AssemblyName) : CompanyName;
            ProductName = string.IsNullOrEmpty(ProductName) ? (FileInfo?.ProductName ?? mainType?.Namespace ?? AssemblyName) : ProductName;
            ProductVersion = string.IsNullOrEmpty(ProductVersion) ? (FileInfo?.ProductVersion ?? FileInfo?.FileVersion ?? "1.0.0") : ProductVersion;
            FileVersion = string.IsNullOrEmpty(FileVersion) ? (FileInfo?.FileVersion ?? "1.0.0") : FileVersion;
            Copyright = string.IsNullOrEmpty(Copyright) ? (FileInfo?.LegalCopyright ?? string.Empty) : Copyright;
            Title = string.IsNullOrEmpty(Title) ? ProductName : Title;
        }

        Description = FileInfo?.FileDescription ?? string.Empty;
        CurrentCulture = string.IsNullOrEmpty(CurrentCulture)
            ? System.Globalization.CultureInfo.CreateSpecificCulture("en-US").Name
            : CurrentCulture;
    }

    public override string ToString() {
        return Utility.Stringify(this, true);
    }

    private void ParseAssembly(Assembly entryAssembly) {
        var attrs = entryAssembly.GetCustomAttributes();
        foreach (var attr in attrs) {
            switch (attr) {
                case AssemblyCompanyAttribute cna:
                    CompanyName = cna.Company;
                    break;
                case AssemblyCopyrightAttribute cpa:
                    Copyright = cpa.Copyright;
                    break;
                case AssemblyProductAttribute pna:
                    ProductName = pna.Product;
                    break;
                case AssemblyTitleAttribute ta:
                    Title = ta.Title;
                    break;
                case AssemblyFileVersionAttribute fva:
                    FileVersion = fva.Version;
                    break;
                case AssemblyInformationalVersionAttribute pva:
                    ProductVersion = pva.InformationalVersion;
                    break;
                case NeutralResourcesLanguageAttribute nrl:
                    CurrentCulture = nrl.CultureName;
                    break;
            }
        }
    }
}