using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CarrotWOL {
    /// <summary>Provides a class for sending wake on lan packets.</summary>
    public class WakeOnLan {
        #region Public Methods

        public static List<IPAddress> FindLocalIPAddress() {
            var results = new List<IPAddress>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces()) {
                if (item.OperationalStatus == OperationalStatus.Up
                    && (item.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    || item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)) {
                    var ap = item.GetIPProperties();
                    var info = ap.GatewayAddresses.FirstOrDefault();
                    if (info == null) {
                        continue;
                    }
                    if (item.Name.Contains("vEthernet") || item.Name.Contains("ZeroTier")) {
                        continue;
                    }
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses) {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork) {
                            results.Add(ip.Address);
                        }
                    }
                }
            }
            return results;
        }

        public static List<IPAddress> GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).ToList();
        }

        /// <summary>Sends a magic packet to a physical address.</summary>
        /// <param name="macAddress">Physical address to send packet to.</param>
        /// <param name="secureOnPassword">Secure on password. (This is sent in clear text!).</param>
        public static void SendMagicPacket(string macString, string? secureOnPassword = null) {
            // build packet: 6 bytes 0xff 16 repetitions of the 6 byte mac address followed by the optional (in)SecureOnPassword
            var macStr = Regex.Replace(macString, "[-|: ]", "-").ToUpper();
            var macAddress= PhysicalAddress.Parse(macStr);
            var size = 17 * 6;
            byte[] buffer;
            if (secureOnPassword == null) {
                buffer = new byte[size];
            } else {
                var passwordBytes = Encoding.ASCII.GetBytes(secureOnPassword);
                buffer = new byte[size + passwordBytes.Length];
                passwordBytes.CopyTo(buffer, size);
            }
            for (var i = 0; i < 6; i++) {
                buffer[i] = 0xff;
            }
            var macBytes = macAddress.GetAddressBytes();
            if (macBytes.Length != 6) {
                throw new ArgumentOutOfRangeException(nameof(macAddress), "Physical mac address bytes out of range (6)!");
            }

            for (var i = 6; i < size; i += 6) {
                macBytes.CopyTo(buffer, i);
            }

            var addresses = FindLocalIPAddress();
            var mask = SubnetMask.CreateByNetBitLength(24);
            addresses.ForEach(addr => {
                var bddr = addr.GetBroadcastAddress(mask);
                Console.WriteLine($"Source: \t{addr}");
                Console.WriteLine($"Destination: \t{addr}");
                Console.WriteLine($"MAC Address: \t{macAddress}");
                var source = new IPEndPoint(addr,0);
                using var udp = new UdpClient(source);
                udp.EnableBroadcast = true;
                foreach (var targetPort in new[] { 0, 7, 9 }) {
                    udp.Send(buffer, buffer.Length, new IPEndPoint(bddr, targetPort));
                }
            });


        }

        #endregion Public Methods
    }
}
