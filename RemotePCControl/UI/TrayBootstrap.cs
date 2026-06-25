#if WINDOWS
using System.Windows.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace RemotePCControl.UI;

public static class TrayBootstrap
{
    public static void Run(IServiceProvider services, IConfiguration config)
    {
        ApplicationConfiguration.Initialize();
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Tray");
        var trayCtx = new TrayApplicationContext(services, config, logger);
        Application.Run(trayCtx);
    }
}
#else
using Microsoft.Extensions.Configuration;

namespace RemotePCControl.UI;

public static class TrayBootstrap
{
    public static void Run(IServiceProvider services, IConfiguration config)
    {
        // Platform stub - tray not available on this OS
    }
}
#endif
