using Microsoft.EntityFrameworkCore;
using DataPacketLibrary;
using DataPacketLibrary.Models;
using RestrictRService;
using System.Security.Principal;
using Microsoft.Extensions.FileProviders;
using Windows.ApplicationModel.Activation;
using Serilog.Events;
using Serilog;
using static System.Environment;


string appName = "RestrictR";
string programDataPath = GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
string logDirectoryPath = Path.Combine(programDataPath, appName, "logs");

// Ensure the log directory exists
Directory.CreateDirectory(logDirectoryPath);

string logFilePath = Path.Combine(logDirectoryPath, "log.txt");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day, shared: true, retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting service host");
    var host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .UseWindowsService(options =>
    {
        options.ServiceName = "RestrictR Blocking Service";
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<RestrictRDbContext>();

        services.AddSingleton<IApplicationBlocker, ApplicationBlocker>();
        services.AddSingleton<IWebsiteBlocker, WebsiteBlocker>();

        services.AddScoped<EventController>();

        services.AddSingleton<IClock, SystemClock>();

        services.AddSingleton<BlockingScheduler>();
        services.AddSingleton<PipeCommunication>();

        services.AddHostedService<Worker>();
    })
    .Build();


    using (var scope = host.Services.CreateScope())
    {
        // essentially used for first time setup - creates the database
        var dbContext = scope.ServiceProvider.GetRequiredService<RestrictRDbContext>();
        dbContext.Database.EnsureCreated();

        // when starting the app firewall and host file should be in a clean state
        var websiteBlocker = scope.ServiceProvider.GetRequiredService<IWebsiteBlocker>();
        websiteBlocker.RemoveBlockedWebsites();
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
