using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CarrotVSNumber {

    internal class Test {

        private static void TestRegexMatch() {
            var name = "AssemblyInfo.cs";
            var input = File.ReadAllText(name);
            //Console.WriteLine(source);
            var pv = @"(\d+\.\d+\.\d+(\.\d+)?)";
            var re = new Regex($@"(^.+AssemblyFileVersion\(""){pv}(""\)\])",  RegexOptions.Multiline);
            var match = re.Match(input);
            Console.WriteLine("Match={0}", match.Value);
            if (match.Success && match.Groups.Count > 4) {
                foreach (var gn in re.GetGroupNames())
                {
                    var g = match.Groups[gn];
                    Console.WriteLine("[{0}] {1}", gn, g.Value);
                }
                var prefix = match.Groups[1].Value;
                var version = match.Groups[2].Value;
                var suffix = match.Groups[4].Value;

                var parts = version.Split('.');
                Console.WriteLine(string.Join(" ", parts));
            }
        }

        private static void TestChangeVersionNumber() {
            Program.ChangeVersionNumber("2.0.1", NumberPattern.AutoInc);
            Program.ChangeVersionNumber("0.0.2", NumberPattern.AutoInc);
            Program.ChangeVersionNumber("2.1.3", NumberPattern.AutoInc);
            Program.ChangeVersionNumber("1.0.4", NumberPattern.AutoInc);
            Program.ChangeVersionNumber("34.355.0", NumberPattern.AutoInc);

            Program.ChangeVersionNumber("2.0.1", NumberPattern.AutoDays);
            Program.ChangeVersionNumber("0.0.2", NumberPattern.AutoDays);
            Program.ChangeVersionNumber("2.1.3", NumberPattern.AutoDays);
            Program.ChangeVersionNumber("1.0.4", NumberPattern.AutoDays);
            Program.ChangeVersionNumber("34.355.0", NumberPattern.AutoDays);

            Program.ChangeVersionNumber("2.0.1", NumberPattern.SetValue, 99);
            Program.ChangeVersionNumber("0.0.2", NumberPattern.SetValue, 99);
            Program.ChangeVersionNumber("2.1.3", NumberPattern.SetValue, 99);
            Program.ChangeVersionNumber("1.0.4", NumberPattern.SetValue, 99);
            Program.ChangeVersionNumber("34.355.0", NumberPattern.SetValue, 99);
        }
    }
}