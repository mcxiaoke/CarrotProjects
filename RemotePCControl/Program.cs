using Serilog;
using System.IO;
using System.Threading.Tasks;
using RemotePCControl.Middleware;
using RemotePCControl.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "rpc-.log"), rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 5_242_880)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Configuration.SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddCommandLine(args)
        .AddEnvironmentVariables();

    var webConfig = builder.Configuration.GetSection("Web").Get<WebSettings>() ?? new WebSettings();
    builder.WebHost.UseUrls($"http://0.0.0.0:{webConfig.Port}");

    builder.Services.AddControllers();
    builder.Services.AddHttpClient();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "RemotePCControl API", Version = "v1" }));

    builder.Services.AddSingleton<ISystemControlService, SystemControlService>();
    builder.Services.AddSingleton<ICommandRouter, CommandRouter>();
    builder.Services.AddSingleton<IBotService, BotService>();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseTokenAuth();
    app.MapControllers();
    app.MapGet("/health", () => new { ok = true, machine = Environment.MachineName, time = DateTime.Now });

    Log.Information("Web host starting at http://0.0.0.0:{port}", webConfig.Port);

    var bot = app.Services.GetRequiredService<IBotService>();
    var router = app.Services.GetRequiredService<ICommandRouter>();

    _ = Task.Run(async () =>
    {
        await bot.StartListening(async cmdText =>
        {
            var result = router.Execute(cmdText);
            if (result.Success) await bot.Notify($"执行: {cmdText} — {result.Message}");
            return result.Message;
        });
    });

    var runTray = OperatingSystem.IsWindows() && !args.Contains("--no-tray");

    if (runTray)
    {
        var webHostTask = app.RunAsync();
        RemotePCControl.UI.TrayBootstrap.Run(app.Services, app.Configuration);
        await webHostTask;
    }
    else
    {
        Log.Information("Running in console / headless mode (no system tray)");
        await app.RunAsync();
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
