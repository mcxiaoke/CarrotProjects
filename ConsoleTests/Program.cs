using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using CarrotCommon;

namespace ConsoleTests {

    internal static class Program {

        private static void Main(string[] args) {
            Console.WriteLine("Hello, World!");
            Assembly exe = Assembly.GetEntryAssembly();
            Console.WriteLine(exe.FullName);
            Console.WriteLine(exe.Location);
            Console.WriteLine("--------");
            var an = exe.GetName();
            Console.WriteLine("an.Version " + an.Version);
            Console.WriteLine("an.Name " + an.Name);
            Console.WriteLine("an.FullName " + an.FullName);
            Console.WriteLine("an.CultureName" + an.CultureName);
            Console.WriteLine("--------");
            var asm = exe.GetCustomAttributes();
            foreach (var a in asm) {
                Console.WriteLine(a);
                if (a is System.Resources.NeutralResourcesLanguageAttribute nl) {
                    Console.WriteLine(nl.CultureName);
                    Console.WriteLine(nl.Location);
                }
            }
            Console.WriteLine("--------");
            var main = exe.EntryPoint.ReflectedType;
            Console.WriteLine(main.Namespace);
            Console.WriteLine(main.Name);
            Console.WriteLine(main.FullName);
            Console.WriteLine("--------");
            CultureInfo cii = CultureInfo.InstalledUICulture;

            Console.WriteLine("Default Language Info:");
            Console.WriteLine("* Name: {0}", cii.Name);
            Console.WriteLine("* Display Name: {0}", cii.DisplayName);
            Console.WriteLine("* English Name: {0}", cii.EnglishName);
            Console.WriteLine("* 2-letter ISO Name: {0}", cii.TwoLetterISOLanguageName);
            Console.WriteLine("* 3-letter ISO Name: {0}", cii.ThreeLetterISOLanguageName);
            Console.WriteLine("* 3-letter Win32 API Name: {0}", cii.ThreeLetterWindowsLanguageName);
            Console.WriteLine("--------");
            CultureInfo ci = CultureInfo.InstalledUICulture;
            Console.WriteLine("Installed Language Info:{0} {1}", ci.Name, ci.EnglishName);
            ci = CultureInfo.CurrentUICulture;
            Console.WriteLine("Current UI Language Info: {0} {1}", ci.Name, ci.EnglishName);
            ci = CultureInfo.CurrentCulture;
            Console.WriteLine("Current Language Info: {0} {1}", ci.Name, ci.EnglishName);
            Console.WriteLine("--------");
            var md = Process.GetCurrentProcess().MainModule;
            Console.WriteLine(md);
            Console.WriteLine(md.FileName);
            Console.WriteLine(md.ModuleName);
            Console.WriteLine("--------");
            Console.WriteLine(md.FileVersionInfo);
            Console.WriteLine("--------");
            Console.WriteLine(AppInfo.AsString());
            Console.WriteLine(AppInfo.CommonAppDataPath);
            Console.WriteLine(AppInfo.LocalAppDataPath);
            Console.WriteLine(AppInfo.RoamingAppDataPath);
            Console.WriteLine("--------");
        }
    }
}