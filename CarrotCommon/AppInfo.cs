using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace CarrotCommon {

    public static class AppInfo {
        private static readonly Lazy<ApplicationData> lazy = new Lazy<ApplicationData>(() => new ApplicationData());
        private static ApplicationData AppData { get { return lazy.Value; } }

        // 获取应用程序的程序集文件的名称（不含扩展名）。
        public static string AssemblyName => AppData.AssemblyName;

        // 可执行文件文件名，包含文件名和扩展名
        public static string ModuleName => AppData.ModuleName;

        // 可执行文件的路径，包括可执行文件的名称。
        public static string ExecutablePath => AppData.ExecutablePath;

        // 可执行文件的路径，不包括可执行文件的名称。
        public static string StartupPath => AppData.StartupPath;

        // 获取与该应用程序关联的公司名称。
        public static string CompanyName => AppData.CompanyName;

        // 获取与应用程序关联的产品名称。
        public static string ProductName => AppData.ProductName;

        // 获取与该应用程序关联的产品版本。
        public static string ProductVersion => AppData.ProductVersion;

        // 获取文件版本号。
        public static string FileVersion => AppData.FileVersion;

        // 获取与应用程序关联的标题。
        public static string Title => AppData.Title;

        // 获取与应用程序关联的版权声明。
        public static string Copyright => AppData.Copyright;

        // 获取与应用程序关联的说明。
        public static string Description => AppData.Description;

        // 获取或设置当前线程的区域性信息。
        public static string CurrentCulture => AppData.CurrentCulture;

        // FileVersionInfo 对象
        public static FileVersionInfo FileInfo => AppData.FileInfo;

        public static string AsString() {
            return AppData.ToString();
        }

        // 获取所有用户共享的应用程序数据的路径。
        public static string CommonAppDataPath
            => GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

        // 获取本地、非漫游用户的应用程序数据的路径。
        public static string LocalAppDataPath => GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

        // 获取用户的应用程序数据的路径。
        public static string RoamingAppDataPath => GetDataPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        // 获取应用程序数据的注册表项。
        public static string AppDataRegistryPath => $"Software\\{CompanyName}\\{ProductName}";

        public static string UserStartupFolder => Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        public static string UserDesktopFolder => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>
        ///  basePath + CompanyName + ProductName + ProductVersion.
        ///  This  will also create the directory if it doesn't exist.
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
        public string AssemblyName { get; }

        public string ModuleName { get; }
        public string ExecutablePath { get; }
        public string StartupPath { get; }
        public string CompanyName { get; private set; }
        public string ProductName { get; private set; }
        public string ProductVersion { get; private set; }
        public string FileVersion { get; private set; }
        public string Title { get; private set; }
        public string Copyright { get; private set; }
        public string Description { get; }
        public string CurrentCulture { get; private set; }

        [JsonIgnore]
        public FileVersionInfo FileInfo { get; }

        public ApplicationData() {
            var module = Process.GetCurrentProcess().MainModule;
            ModuleName = module.ModuleName;
            FileInfo = module.FileVersionInfo;
            StartupPath = AppContext.BaseDirectory;

            Assembly entryAssembly = Assembly.GetEntryAssembly();
            var mainType = entryAssembly.EntryPoint?.ReflectedType;
            ExecutablePath = entryAssembly.Location;
            AssemblyName = entryAssembly.GetName().Name;
            ParseAssembly(entryAssembly);
            Description = FileInfo.FileDescription ?? string.Empty;
            if (string.IsNullOrWhiteSpace(CompanyName)) {
                CompanyName = FileInfo.CompanyName ?? mainType.Namespace;
            }
            if (string.IsNullOrWhiteSpace(ProductName)) {
                ProductName = FileInfo.ProductName ?? mainType.Namespace;
            }
            if (string.IsNullOrWhiteSpace(ProductVersion)) {
                ProductVersion = FileInfo.ProductVersion ?? FileInfo.FileVersion ?? "1.0.0";
            }
            if (string.IsNullOrWhiteSpace(FileVersion)) {
                FileVersion = FileInfo.FileVersion ?? "1.0.0";
            }
            if (string.IsNullOrWhiteSpace(Copyright)) {
                Copyright = FileInfo.LegalCopyright ?? string.Empty;
            }
            if (string.IsNullOrWhiteSpace(Title)) {
                Title = ProductName;
            }
        }

        public override string ToString() {
            return Utility.Stringify(this, true);
        }

        private void ParseAssembly(Assembly entryAssembly) {
            //System.Reflection.AssemblyCompanyAttribute
            //System.Reflection.AssemblyConfigurationAttribute
            //System.Reflection.AssemblyCopyrightAttribute
            //System.Reflection.AssemblyFileVersionAttribute
            //System.Reflection.AssemblyInformationalVersionAttribute
            //System.Reflection.AssemblyProductAttribute
            //System.Reflection.AssemblyTitleAttribute
            // System.Resources.NeutralResourcesLanguageAttribute
            var attrs = entryAssembly.GetCustomAttributes();
            foreach (var attr in attrs) {
                if (attr is AssemblyCompanyAttribute cna) {
                    CompanyName = cna.Company;
                } else if (attr is AssemblyCopyrightAttribute cpa) {
                    Copyright = cpa.Copyright;
                } else if (attr is AssemblyProductAttribute pna) {
                    ProductName = pna.Product;
                } else if (attr is AssemblyTitleAttribute ta) {
                    Title = ta.Title;
                } else if (attr is AssemblyFileVersionAttribute fva) {
                    FileVersion = fva.Version;
                } else if (attr is AssemblyInformationalVersionAttribute pva) {
                    ProductVersion = pva.InformationalVersion;
                } else if (attr is System.Resources.NeutralResourcesLanguageAttribute nrl) {
                    CurrentCulture = nrl.CultureName;
                }
            }
        }
    }
}