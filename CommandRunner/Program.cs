using CommandRunner.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// 功能
// 无控制台窗口运行各类脚本，相当于vbs的作用
// 支持命令行和参数
namespace CommandRunner {

    static class OSHelper {

        private const int MAX_PATH = 260;
        /// <summary>
        /// Expands environment variables and, if unqualified, locates the exe in the working directory
        /// or the evironment's path.
        /// </summary>
        /// <param name="exe">The name of the executable file</param>
        /// <returns>The fully-qualified path to the file</returns>
        /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
        public static string FindExecutableInPath(string exe) {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe)) {
                if (Path.GetDirectoryName(exe) == String.Empty) {
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';')) {
                        string path = test.Trim();
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path);
                    }
                }
                throw new FileNotFoundException(new FileNotFoundException().Message, exe);
            }
            return Path.GetFullPath(exe);
        }

        /// <summary>
        /// Gets the full path of the given executable filename as if the user had entered this
        /// executable in a shell. So, for example, the Windows PATH environment variable will
        /// be examined. If the filename can't be found by Windows, null is returned.</summary>
        /// <param name="exeName"></param>
        /// <returns>The full path if successful, or null otherwise.</returns>
        public static string FindExecutableFullPath(string exeName) {
            if (exeName.Length >= MAX_PATH)
                throw new ArgumentException($"The executable name '{exeName}' must have less than {MAX_PATH} characters.",
                    nameof(exeName));

            StringBuilder sb = new StringBuilder(exeName, MAX_PATH);
            return PathFindOnPath(sb, null) ? sb.ToString() : null;
        }

        // https://learn.microsoft.com/en-us/windows/desktop/api/shlwapi/nf-shlwapi-pathfindonpathw
        // https://www.pinvoke.net/default.aspx/shlwapi.PathFindOnPath
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, SetLastError = false)]
        static extern bool PathFindOnPath([In, Out] StringBuilder pszFile, [In] string[] ppszOtherDirs);
    }

    internal static class Program {

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //todo 找到配置文件并运行，然后自己退出
            // 配置文件顺序
            // app_name.json
            // app_name.ini
            // app_name.txt
            // app_name.bat
            // app_name.cmd
            // app_name.js
            // app_name.py
            MessageBox.Show(Application.ExecutablePath);


            Application.Run(new MyApplicationContext());
        }

        // https://stackoverflow.com/questions/5519328
        static void RunCommand(string fileName, string arguments, string workingDirectory) {
            string workingDir = string.IsNullOrEmpty(workingDirectory) ? Application.StartupPath : workingDirectory;

            string cmdName = string.IsNullOrEmpty(fileName) ? "cmd.exe" : fileName;
            string cmdArgs = string.IsNullOrEmpty(fileName)?"/C "+arguments:arguments;
            
            ProcessStartInfo info = new ProcessStartInfo(cmdName, cmdArgs) {
                WorkingDirectory = workingDir,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            Process process = new Process { StartInfo = info};

            int exitCode = -1;
            string output = null;
            string error = null;

            try {       
                process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
                    output = e.Data;
                    Console.WriteLine("output>>" + e.Data);
                };
                
                process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => {
                    exitCode = -1;
                    error = e.Data;
                    Console.WriteLine("error>>" + e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                // MessageBox.Show("Bat file executed...");
            } catch (Exception e) {
                Console.WriteLine("error>>" + e.Data);
            } finally {
                exitCode = process.ExitCode;
                process.Close();
                process.Dispose();
                process = null;
            }
        }

    }

    public class MyApplicationContext : ApplicationContext {
        private NotifyIcon trayIcon;

        public MyApplicationContext() {
            trayIcon = new NotifyIcon() {
                Icon = Resources.App,
                ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", Exit),
                new MenuItem("Dialog",ShowDialog)
            }),
                Visible = true
            };
        }

        void Exit(object sender, EventArgs e) {
            trayIcon.Visible = false;
            Application.Exit();
        }

        void ShowDialog(object sender, EventArgs e) {
            new AlertDialog().ShowDialog();
        }
    }
}
