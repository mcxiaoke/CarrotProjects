namespace Carrot.AutoLock {
    internal static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

#if (NET6_0_OR_GREATER)
            //Console.WriteLine("Using .NET 6+ or .NET Standard 2+ code.");
            ApplicationConfiguration.Initialize();
#else
            //Console.WriteLine("Using older code that doesn't support the above .NET versions.");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            Console.WriteLine(AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName);
            Application.Run(new MainForm());
        }
    }
}