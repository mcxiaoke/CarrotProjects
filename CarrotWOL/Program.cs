using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace CarrotWOL {

    public static class Program {

        public static void Main(string[] args) {
            var appName = System.AppDomain.CurrentDomain.FriendlyName;
            var usageStr = $"Usage: {appName} " +
                    $"mac [mac address of wol target machine]\n"+
                    "Format: a1:b2:c3:d4:e5:f6 or a1-b2-c3-d4-e5-f6 or a1b2c3d4e5f6\n";
            if (args.Length < 1 || string.IsNullOrWhiteSpace(args[0])) {
                Console.Error.WriteLine($"\nError: No mac address argument found!");
                Console.Error.WriteLine(usageStr);
                return;
            }
            string macString = args[0];
            try {
                var macStr = Regex.Replace(macString, "[-|: ]", "-").ToUpper();
                PhysicalAddress.Parse(macStr);
            } catch (Exception) {
                Console.Error.WriteLine($"Invalid MAC Address: {macString}");
                Console.Error.WriteLine(usageStr);
                return;
            }
            try {
                WakeOnLan.SendMagicPacket(macString);
                Console.WriteLine($"WOL magic packet is sent to {macString}");
            } catch (Exception e) {
                Console.Error.WriteLine($"WOL magic packet failed: {macString} {e}");
            }
        }
    }
}