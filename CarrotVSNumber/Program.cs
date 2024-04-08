using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CommandLine;

namespace CarrotVSNumber {
    //Match=[assembly: AssemblyFileVersion("2.0.8.23")]
    //[0] [assembly: AssemblyFileVersion("2.0.8.23")]
    //[1] [assembly: AssemblyFileVersion("
    //[2] 2.0.8.23
    //[3] .23
    //[4] ")]

    public class Options {

        [Option('f', "file", Required = false,
            HelpText = "AssemblyInfo.cs file to change version number")]
        public string? Target { get; set; }

        [Option('b', "backup", Required = false, HelpText = "bakup old AssemblyInfo.cs file")]
        public bool Backup { get; set; }

        [Option('y', "yes", Required = false, HelpText = "use this option to do real version number change")]
        public bool Yes { get; set; }

        [Option('i', "inc", Required = false, HelpText = "set version number by integer autoincrement")]
        public bool AutoInc { get; set; }

        [Option('d', "days", Required = false, HelpText = "set version number by days from 2022-01-01")]
        public bool AutoDays { get; set; }

        [Option('v', "value", Default = -1, Required = false, HelpText = "set version number by command argument (0~65535)")]
        public int SetValue { get; set; }

        public override string ToString() {
            return $"Options(Target={Target},Yes={Yes}," +
                $"AutoInc={AutoInc},AutoDays={AutoDays}," +
                $"SetValue={SetValue},Backup={Backup})";
        }
    }

    // https://docs.microsoft.com/zh-cn/visualstudio/msbuild/property-functions?view=vs-2022
    // https://docs.microsoft.com/zh-cn/dotnet/standard/base-types/the-regular-expression-object-model
    internal static class Program {

        private static void ShowHelp() {
            Core.Log($"Please run again with --help for more information.");
        }

        private static void Main(string[] args) {
            var parser = new CommandLine.Parser();
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            parserResult.WithParsed(HandleCommand)
                .WithNotParsed(HandleError);
        }

        private static void HandleError(object error) {
            // ignore errors
            //Console.WriteLine(error);
        }

        private static void HandleCommand(Options o) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var companyName = fvi.CompanyName;
            var productName = fvi.ProductName;
            var productVersion = fvi.ProductVersion;
            Console.WriteLine("############################################");
            Console.WriteLine($"### {productName} {productVersion} ({companyName}) ###");
            Console.WriteLine("############################################");
            Core.Log($"command: {o}");
            //todo
            // need detect project frameworkd
            // .net and .net core do not use AssemblyInfo.cs
            // they use FileVersion property in .csproj xml file
            // distinct by TargetFramework and root Project property
            var filepath = o.Target ?? @"Properties\AssemblyInfo.cs";
            var file = Path.GetFullPath(filepath);
            if (!File.Exists(file)) {
                Core.Error($"1001 Target file '{file}' not exists");
                ShowHelp();
                Environment.Exit(1001);
                return;
            }
            Core.Log("file:", file);
            var np = NumberPattern.None;
            if (o.SetValue >= 0 && o.SetValue <= 65535) {
                np = NumberPattern.SetValue;
            } else if (o.AutoInc) {
                np = NumberPattern.AutoInc;
            } else if (o.AutoDays) {
                np = NumberPattern.AutoDays;
            }
            Core.Log("pattern:", np.ToString());
            if (np == NumberPattern.None) {
                Core.Error($"1002 No valid version number pattern, nothing to do");
                ShowHelp();
                Environment.Exit(1002);
                return;
            }
            var oldContent = File.ReadAllText(file, Encoding.UTF8);
            var newContent = ReplaceVersionNumber(oldContent, np, o.SetValue);
            if (!o.Yes) {
                Core.Log($"No changes made, dry run mode, use -y/--yes option to change orginal file");
                Environment.Exit(0);
                return;
            }
            if (oldContent == newContent) {
                Core.Log($"Version number nothing to change");
                return;
            }

            try {
                if (o.Backup) {
                    var bakFile = file + ".bak";
                    if (File.Exists(bakFile)) {
                        File.Delete(bakFile);
                    }
                    File.Move(file, bakFile);
                    Core.Log($"Backup AssemblyInfo.cs file");
                } else {
                    //File.Delete(file);
                }
                File.WriteAllText(file, newContent, Encoding.UTF8);
                Core.Log("All done, orignal file modified, version number changed");
                Environment.Exit(0);
            } catch (Exception ex) {
                Core.Error($"1003 Failed to write file: {ex.Message}");
                ShowHelp();
                Environment.Exit(1003);
            }
        }

        public static string ReplaceVersionNumber(string content, NumberPattern np, int newValue) {
            const string pv = @"(\d+\.\d+\.\d+(\.\d+)?)";
            string pattern = $@"(^.+AssemblyFileVersion\(""){pv}(""\)\])";
            var match = Regex.Match(content, pattern, RegexOptions.Multiline);
            //Core.Log("Match={0}", match.Value);
            if (match.Success && match.Groups.Count > 4) {
                //foreach (Group g in match.Groups) {
                //Console.WriteLine("[{0}] {1}", g.Name, g.Value);
                //}
                Core.Log("Old:", match.Value);
                var prefix = match.Groups[1].Value;
                var numberStr = match.Groups[2].Value;
                var suffix = match.Groups[4].Value;
                var numberAfter = ChangeVersionNumber(numberStr, np, newValue);
                Core.Log("New:", prefix + numberAfter + suffix);
                if (numberAfter != numberStr) {
                    return content.Replace(prefix + numberStr + suffix, prefix + numberAfter + suffix);
                }
            } else {
                Core.Error("Version number string not found");
            }
            return content;
        }

        public static string ChangeVersionNumber(string numberStr, NumberPattern cp, int newValue = -1) {
            var parts = numberStr.Split('.');
            var lastStr = parts[parts.Length - 1];
            var lastNum = Core.ParseInt(lastStr);
            if (lastNum == Int32.MinValue) {
                // invalid number, just return
                return numberStr;
            }
            switch (cp) {
                case NumberPattern.AutoInc:
                    ++lastNum;
                    break;

                case NumberPattern.AutoDays:
                    lastNum = Core.GetDaysFromYear2022;
                    break;

                case NumberPattern.SetValue:
                    if (newValue >= 0 && newValue <= 65535) {
                        lastNum = newValue;
                    }
                    break;

                case NumberPattern.None:
                default:
                    break;
            }
            lastStr = lastNum.ToString();
            parts[parts.Length - 1] = lastStr;
            return string.Join(".", parts);
        }
    }
}