using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;

namespace SharpUpdater {

    public class CommandOptions {
        [Option('v', "verbose", Required = false, HelpText = "输出详细的信息")]
        public bool Verbose { get; set; }

        [Option('n', "name", Required = false, HelpText = "检查更新的应用程序名称")]
        public string Name { get; set; }

        [Option('u', "url", Required = false, HelpText = "检查更新的网络配置文件地址")]
        public string URL { get; set; }

        [Option('c', "config", Required = false, HelpText = "更新工具的本地配置文件")]
        public string ConfigFile { get; set; }

        public override string ToString() {
            return $"CommandOptions(Name={Name} URL={URL} ConfigFile={ConfigFile})";
        }
    }

    internal static class Program {


        [STAThread]
        static void Main(string[] args) {
            Parser.Default.ParseArguments<CommandOptions>(args)
              .WithParsed(options => {
                  //Console.WriteLine(options);
                  Application.EnableVisualStyles();
                  Application.SetCompatibleTextRenderingDefault(false);
                  Application.Run(new UpdateDialog(options));
              })
              .WithNotParsed(errors => {
                  Application.Exit();
              });
        }
    }
}
