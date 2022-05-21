using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace GenshinNotifier {
    public class Storage {

        public static string AppName => Application.ProductName;
        public static string AppVersion => Application.ProductVersion;
        public static string AppGuidStr => AppGuid.ToString("B").ToUpper();

        /// <summary>
        /// Get the Application Guid
        /// </summary>
        public static Guid AppGuid {
            get {
                Assembly asm = Assembly.GetEntryAssembly();
                object[] attr = (asm.GetCustomAttributes(typeof(GuidAttribute), true));
                return new Guid((attr[0] as GuidAttribute).Value);
            }
        }
        /// <summary>
        /// Get the current assembly Guid.
        /// <remarks>
        /// Note that the Assembly Guid is not necessarily the same as the
        /// Application Guid - if this code is in a DLL, the Assembly Guid
        /// will be the Guid for the DLL, not the active EXE file.
        /// </remarks>
        /// </summary>
        public static Guid AssemblyGuid {
            get {
                Assembly asm = Assembly.GetExecutingAssembly();
                object[] attr = (asm.GetCustomAttributes(typeof(GuidAttribute), true));
                return new Guid((attr[0] as GuidAttribute).Value);
            }
        }
        /// <summary>
        /// Get the current user data folder
        /// </summary>
        public static string UserDataFolder {
            get {
                string folderBase = Environment.GetFolderPath
                                    (Environment.SpecialFolder.LocalApplicationData);
                string dir = Path.Combine(folderBase, AppGuidStr);
                return CheckDir(dir);
            }
        }
        /// <summary>
        /// Get the current user roaming data folder
        /// </summary>
        public static string UserRoamingDataFolder {
            get {
                string folderBase = Environment.GetFolderPath
                                    (Environment.SpecialFolder.ApplicationData);
                string dir = Path.Combine(folderBase, AppGuidStr);
                return CheckDir(dir);
            }
        }
        /// <summary>
        /// Get all users data folder
        /// </summary>
        public static string AllUsersDataFolder {
            get {
                string folderBase = Environment.GetFolderPath
                                    (Environment.SpecialFolder.CommonApplicationData);
                string dir = Path.Combine(folderBase, AppGuidStr);
                return CheckDir(dir);
            }
        }
        /// <summary>
        /// Check the specified folder, and create if it doesn't exist.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static string CheckDir(string dir) {
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        public static string UserStartupFolder => Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        public static string UserDesktopFolder => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    }
}
