// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Carrot.Device;
using DeviceId;
using DeviceId.Encoders;
using DeviceId.Formatters;
using LabsConsole;

string deviceId = new DeviceIdBuilder()
    .AddUserName()
    .AddMachineName()
    .AddOsVersion()
    .OnWindows(windows => windows
        .AddProcessorId()
        .AddMachineGuid()
        .AddMacAddressFromWmi(excludeWireless: true, excludeNonPhysical: true)
        .AddMotherboardSerialNumber()
        .AddSystemDriveSerialNumber()
        .AddSystemUuid())
    .UseFormatter(new HashDeviceIdFormatter(() => SHA1.Create(), new HexByteArrayEncoder()))
    .ToString();
Console.WriteLine(deviceId);

Network.ShowNetworkInterfaces();

// Instantiate the explorer class
Explorer explorer = new Explorer();

// If the results logging is required, the file output should be enabled (disabled by default)
Globals.Enable_File_Output = true;

// The default log filename is "devices.txt". It can be altered if required as follows:
Globals.Output_Filename = "device.txt";

// Invoke the run method to retrieve information from WMI structure (this may take few seconds)
// explorer.Run();



Console.WriteLine(Environment.OSVersion);
Console.WriteLine($"Version: {Environment.Version}");
Console.WriteLine(RuntimeInformation.FrameworkDescription);
OS.ShowFrameworks();
Console.WriteLine(Environment.GetFolderPath(Environment.SpecialFolder.System));
