using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Carrot.Common;

namespace CarrotHashCli;

/// <summary>
/// CarrotHash CLI - A fast file hash computation tool.
/// </summary>
public static class Program {
    // Constants
    private const long FILE_SIZE_1GB = 1024L * 1024 * 1024;
    private const long FILE_SIZE_100MB = 100L * 1024 * 1024;
    private const int LABEL_WIDTH = 10;
    private const string SEPARATOR = "------------------------";

    public static void Main(string[] args) {
        if (args.Length < 1 || args.All(string.IsNullOrWhiteSpace)) {
            PrintUsage();
            return;
        }

        var files = args.Where(arg => !string.IsNullOrWhiteSpace(arg)).ToList();
        
        Console.WriteLine($"\nComputing hash for {files.Count} file(s)...\n");

        for (int i = 0; i < files.Count; i++) {
            Console.WriteLine($"[{i + 1}/{files.Count}]");
            HashFile(files[i]);
        }
    }

    private static void PrintUsage() {
        Console.WriteLine("CarrotHash CLI v1.1 - Fast File Hash Computation Tool");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  CarrotHashCli.exe <file1> [file2] [file3] ...");
        Console.WriteLine();
        Console.WriteLine("Supported algorithms: MD5, SHA1, SHA256, SHA384, SHA512");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  CarrotHashCli.exe myfile.txt");
        Console.WriteLine("  CarrotHashCli.exe file1.zip file2.exe file3.dll");
    }

    private static void HashFile(string filepath) {
        var stopwatch = Stopwatch.StartNew();
        var file = new FileInfo(filepath);

        Console.WriteLine(SEPARATOR);
        PrintInfo("Path:", file.FullName);

        // Validate file
        if (!file.Exists) {
            PrintError("File not found or not accessible");
            return;
        }

        // Check file size
        if (file.Length > FILE_SIZE_1GB) {
            var gbSize = (double)file.Length / FILE_SIZE_1GB;
            PrintWarning($"Large file ({gbSize:F2} GB) - computation may take a while");
            
            if (!PromptContinue()) {
                PrintInfo("Action:", "Skipped by user");
                return;
            }
        }

        // Display file info
        PrintInfo("Name:", file.Name);
        PrintInfo("Size:", FormatFileSize(file.Length));
        PrintInfo("Modified:", file.LastWriteTime.ToString("s", CultureInfo.InvariantCulture));

        // Compute hashes
        Console.WriteLine();
        try {
            var hashes = ComputeAllHashes(file.FullName);
            
            foreach (var (algorithm, hash) in hashes) {
                PrintInfo($"{algorithm}:", hash);
            }
        }
        catch (IOException ex) {
            PrintError($"I/O error: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex) {
            PrintError($"Access denied: {ex.Message}");
        }
        catch (Exception ex) {
            PrintError($"Error: {ex.Message}");
        }

        // Show elapsed time
        stopwatch.Stop();
        if (stopwatch.Elapsed.TotalSeconds > 1) {
            PrintInfo("Time:", $"{stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }
        
        Console.WriteLine();
    }

    /// <summary>
    /// Computes all hash algorithms in parallel for better performance.
    /// </summary>
    private static Dictionary<string, string> ComputeAllHashes(string filepath) {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Use parallel computation for better performance
        Parallel.Invoke(
            () => result["MD5"] = CarrotHash.FileMD5(filepath),
            () => result["SHA1"] = CarrotHash.FileSHA1(filepath),
            () => result["SHA256"] = CarrotHash.FileSHA256(filepath),
            () => result["SHA384"] = CarrotHash.FileSHA384(filepath),
            () => result["SHA512"] = CarrotHash.FileSHA512(filepath)
        );

        return result;
    }

    private static bool PromptContinue() {
        Console.Write("Continue? (y/n): ");
        var answer = Console.ReadLine();
        return string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(answer, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static void PrintInfo(string label, string value) {
        Console.WriteLine($"{label.Fixed(LABEL_WIDTH)}{value}");
    }

    private static void PrintWarning(string message) {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"{"Warning:".Fixed(LABEL_WIDTH)}{message}");
        Console.ForegroundColor = color;
    }

    private static void PrintError(string message) {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"{"Error:".Fixed(LABEL_WIDTH)}{message}");
        Console.ForegroundColor = color;
    }

    private static string FormatFileSize(long bytes) {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1) {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:F2} {units[unitIndex]}";
    }
}

/// <summary>
/// String extension methods for formatting.
/// </summary>
public static class StringExtensions {

    /// <summary>
    /// Pads or truncates a string to a fixed width.
    /// </summary>
    public static string Fixed(this string value, int totalWidth, char paddingChar = ' ') {
        if (string.IsNullOrEmpty(value)) {
            return new string(paddingChar, totalWidth);
        }

        return value.Length > totalWidth
            ? value.Substring(0, totalWidth)
            : value.PadRight(totalWidth, paddingChar);
    }
}
