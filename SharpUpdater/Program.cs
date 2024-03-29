﻿using System;
using System.Windows.Forms;
using CommandLine;

namespace SharpUpdater {

    public class CommandOptions {

        [Option('v', "verbose", Required = false, HelpText = "输出详细的信息")]
        public bool Verbose { get; set; }

        [Option('n', "name", Required = false, HelpText = "检查更新的应用程序名称")]
        public string Name { get; set; } = String.Empty;

        [Option('u', "url", Required = false, HelpText = "检查更新的网络配置文件地址")]
        public string URL { get; set; } = String.Empty;

        [Option('c', "config", Required = false, HelpText = "更新工具的本地配置文件")]
        public string ConfigFile { get; set; } = String.Empty;

        public override string ToString() {
            return $"CommandOptions(Name={Name} URL={URL} ConfigFile={ConfigFile})";
        }
    }

    internal static class Program {

        [STAThread]
        private static void Main(string[] args) {
            Parser.Default.ParseArguments<CommandOptions>(args)
              .WithParsed(options => {
                  //Console.WriteLine(options);
                  Application.EnableVisualStyles();
                  Application.SetCompatibleTextRenderingDefault(false);
                  Application.Run(new UpdateDialog(options));
              })
              .WithNotParsed(_ => Application.Exit());
        }
    }
}