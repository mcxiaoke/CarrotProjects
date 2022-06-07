using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LabsConsole {
    static class Network {


        public static void ShowIPAddresses(IPInterfaceProperties adapterProperties) {
            IPAddressCollection dnsServers = adapterProperties.DnsAddresses;
            if (dnsServers != null) {
                foreach (IPAddress dns in dnsServers) {
                    Console.WriteLine("  DNS Servers ............................. : {0}",
                        dns.ToString()
                   );
                }
            }
            IPAddressInformationCollection anyCast = adapterProperties.AnycastAddresses;
            if (anyCast != null) {
                foreach (IPAddressInformation any in anyCast) {
                    Console.WriteLine("  Anycast Address .......................... : {0} {1} {2}",
                        any.Address,
                        any.IsTransient ? "Transient" : "",
                        any.IsDnsEligible ? "DNS Eligible" : ""
                    );
                }
                Console.WriteLine();
            }

            MulticastIPAddressInformationCollection multiCast = adapterProperties.MulticastAddresses;
            if (multiCast != null) {
                foreach (IPAddressInformation multi in multiCast) {
                    Console.WriteLine("  Multicast Address ....................... : {0} {1} {2}",
                        multi.Address,
                        multi.IsTransient ? "Transient" : "",
                        multi.IsDnsEligible ? "DNS Eligible" : ""
                    );
                }
                Console.WriteLine();
            }
            UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
            if (uniCast != null) {
                string lifeTimeFormat = "dddd, MMMM dd, yyyy  hh:mm:ss tt";
                foreach (UnicastIPAddressInformation uni in uniCast) {
                    DateTime when;

                    Console.WriteLine("  Unicast Address ......................... : {0}", uni.Address);
                    Console.WriteLine("     Prefix Origin ........................ : {0}", uni.PrefixOrigin);
                    Console.WriteLine("     Suffix Origin ........................ : {0}", uni.SuffixOrigin);
                    Console.WriteLine("     Duplicate Address Detection .......... : {0}",
                        uni.DuplicateAddressDetectionState);

                    // Format the lifetimes as Sunday, February 16, 2003 11:33:44 PM
                    // if en-us is the current culture.

                    // Calculate the date and time at the end of the lifetimes.
                    when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressValidLifetime);
                    when = when.ToLocalTime();
                    Console.WriteLine("     Valid Life Time ...................... : {0}",
                        when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture)
                    );
                    when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.AddressPreferredLifetime);
                    when = when.ToLocalTime();
                    Console.WriteLine("     Preferred life time .................. : {0}",
                        when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture)
                    );

                    when = DateTime.UtcNow + TimeSpan.FromSeconds(uni.DhcpLeaseLifetime);
                    when = when.ToLocalTime();
                    Console.WriteLine("     DHCP Leased Life Time ................ : {0}",
                        when.ToString(lifeTimeFormat, System.Globalization.CultureInfo.CurrentCulture)
                    );
                }
                Console.WriteLine();
            }
        }

        public static void ShowNetworkInterfaces(bool EthernetOnly = true) {
            IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            var nics = NetworkInterface.GetAllNetworkInterfaces().ToList();
            Console.WriteLine("Interface information for {0}.{1}     ",
                    computerProperties.HostName, computerProperties.DomainName);
            if (nics == null || nics.Count < 1) {
                Console.WriteLine("  No network interfaces found.");
                return;
            }

            nics = nics.FindAll(it => it.NetworkInterfaceType == NetworkInterfaceType.Ethernet);

            Console.WriteLine("  Number of interfaces .................... : {0}", nics.Count);
            foreach (NetworkInterface adapter in nics) {
                IPInterfaceProperties properties = adapter.GetIPProperties();
                Console.WriteLine();
                Console.WriteLine(adapter.Description);
                Console.WriteLine(String.Empty.PadLeft(adapter.Description.Length, '='));
                Console.WriteLine("  Interface type .......................... : {0}", adapter.NetworkInterfaceType);
                Console.WriteLine("  Physical Address ........................ : {0}",
                           adapter.GetPhysicalAddress().ToString());
                Console.WriteLine("  Operational status ...................... : {0}",
                    adapter.OperationalStatus);
                string versions = "";

                // Create a display string for the supported IP versions.
                if (adapter.Supports(NetworkInterfaceComponent.IPv4)) {
                    versions = "IPv4";
                }
                if (adapter.Supports(NetworkInterfaceComponent.IPv6)) {
                    if (versions.Length > 0) {
                        versions += " ";
                    }
                    versions += "IPv6";
                }
                Console.WriteLine("  IP version .............................. : {0}", versions);
                ShowIPAddresses(properties);

                // The following information is not useful for loopback adapters.
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback) {
                    continue;
                }
                Console.WriteLine("  DNS suffix .............................. : {0}",
                    properties.DnsSuffix);

                string label;
                if (adapter.Supports(NetworkInterfaceComponent.IPv4)) {
                    IPv4InterfaceProperties ipv4 = properties.GetIPv4Properties();
                    Console.WriteLine("  MTU...................................... : {0}", ipv4.Mtu);
                    if (ipv4.UsesWins) {

                        IPAddressCollection winsServers = properties.WinsServersAddresses;
                        if (winsServers.Count > 0) {
                            label = "  WINS Servers ............................ :";
                            //ShowIPAddresses(label, winsServers);
                        }
                    }
                }

                Console.WriteLine("  DNS enabled ............................. : {0}",
                    properties.IsDnsEnabled);
                Console.WriteLine("  Dynamically configured DNS .............. : {0}",
                    properties.IsDynamicDnsEnabled);
                Console.WriteLine("  Receive Only ............................ : {0}",
                    adapter.IsReceiveOnly);
                Console.WriteLine("  Multicast ............................... : {0}",
                    adapter.SupportsMulticast);
                //ShowInterfaceStatistics(adapter);

                Console.WriteLine();
            }
        }
    }
}
