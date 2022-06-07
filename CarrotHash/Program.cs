using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CarrotCommon;

namespace CarrotHashCli {

    public static class Extensions {

        public static string Fixed(this string value, int totalWidth, char paddingChar = ' ') {
            if (value is null)
                return new string(paddingChar, totalWidth);

            if (value.Length > totalWidth)
                return value.Substring(0, totalWidth);
            else
                return value.PadRight(totalWidth, paddingChar);
        }
    }

    public class Program {
        private const long FILE_SIZE_1G = 1024 * 1024 * 1024;

        private static void HashFile(string filepath) {
            var file = new FileInfo(filepath);
            Console.WriteLine();
            Console.WriteLine("--------------------");
            Console.WriteLine("{0,-10} {1}", "Path:".Fixed(10), file.FullName);
            if (!file.Exists) {
                Console.WriteLine("{0,-10} {1}\n", "Error:".Fixed(10), "Not exists or not regular file");
                return;
            }

            // https://docs.microsoft.com/zh-cn/dotnet/standard/base-types/standard-numeric-format-strings
            // https://docs.microsoft.com/zh-cn/dotnet/standard/base-types/composite-formatting
            // https://docs.microsoft.com/zh-cn/dotnet/csharp/language-reference/tokens/interpolated
            if (file.Length > FILE_SIZE_1G) {
                decimal gbSize = Convert.ToDecimal(file.Length) / FILE_SIZE_1G;
                Console.WriteLine("{0,-10} {1}", "Warning:".Fixed(10), $"File is too large, over {gbSize:F3}GB, " +
                    $"computing hash may take a long time, contine? (yes/no)");
                Console.Write("{0,-10}", "Choice: ");
                var answer = Console.ReadLine();
                if (answer.ToLower() != "yes") {
                    Console.WriteLine("{0,-10} {1}", "Action:", "User abort.");
                    return;
                }
            }

            Console.WriteLine("{0,-10} {1}", "Name:", file.Name);
            Console.WriteLine("{0,-10} {1}", "Size:", file.Length);
            // https://stackoverflow.com/questions/114983
            // https://code-maze.com/convert-datetime-to-iso-8601-string-csharp/
            Console.WriteLine("{0,-10} {1}", "Creation:", file.CreationTime.ToString("s", CultureInfo.InvariantCulture));
            Console.WriteLine("{0,-10} {1}", "Modified:", file.LastWriteTime.ToString("s", CultureInfo.InvariantCulture));
            var timeStart = DateTime.Now;
            var md5 = CarrotHash.FileMD5(filepath);
            var sha1 = CarrotHash.FileSHA1(filepath);
            var sha256 = CarrotHash.FileSHA256(filepath);
            Console.WriteLine("{0,-10} {1}", "MD5:", md5);
            Console.WriteLine("{0,-10} {1}", "SHA1:", sha1);
            Console.WriteLine("{0,-10} {1}", "SHA256:", sha256);
            var elapsed = DateTime.Now - timeStart;
            if (elapsed.TotalSeconds > 10) {
                Console.WriteLine("{0,-10} {1} seconds", "Elapsed:", Convert.ToInt32(elapsed.TotalSeconds));
            }
        }

        public static void Main(string[] args) {
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0])) {
                Console.WriteLine($"Usage: CarrotHashCli.exe myfile1.txt myfile2.exe myfile3.zip");
                return;
            }
            args.ToList().FindAll(it => !string.IsNullOrWhiteSpace(it)).ForEach(it => HashFile(it));
        }
    }
}