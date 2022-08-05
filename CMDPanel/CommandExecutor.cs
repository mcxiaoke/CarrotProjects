using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using PropertyChanged;

namespace CMDPanel {
    public class CommandExecutor : INotifyPropertyChanged {
        public const string STR_EMPTY = "";

        public EventHandler<DataReceivedEventArgs>? OutputHandler;
        public EventHandler<int>? ExitHandler;

        public string FileName { get; set; } = STR_EMPTY;
        public string Arguments { get; set; } = STR_EMPTY;
        private ProcessStartInfo? StartInfo;
        private Process? Proc;

        public event PropertyChangedEventHandler? PropertyChanged;

        public CommandExecutor(string fileName, string arguments = STR_EMPTY) {
            FileName = fileName;
            Arguments = arguments;
        }

        public CommandExecutor() {
        }

        public void Start() {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = FileName;
            info.Arguments = Arguments;
            info.UseShellExecute = false;
            info.CreateNoWindow = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.RedirectStandardInput = true;
            using Process p = new Process();
            p.StartInfo = info;
            p.OutputDataReceived += P_OutputDataReceived;
            p.ErrorDataReceived += P_ErrorDataReceived;
            p.Exited += P_Exited;
            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            Debug.WriteLine($"{FileName} Start");
            this.StartInfo = info;
            this.Proc = p;
        }

        private void P_Exited(object sender, EventArgs e) {
            Debug.WriteLine($"{FileName} P_Exited");
            ExitHandler?.Invoke(sender, Proc?.ExitCode ?? -1);
        }

        private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e) {
            Debug.WriteLine($"{FileName} P_ErrorDataReceived");
            OutputHandler?.Invoke(sender, e);
        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            Debug.WriteLine($"{FileName} P_OutputDataReceived");
            OutputHandler?.Invoke(sender, e);
        }

        public void Stop() {
            if (Proc is Process p) {
                p.Close();
                Debug.WriteLine($"{FileName} Stop");
            }
        }

        public bool IsRunning => Proc is Process p && !p.HasExited;
    }
}
