using Microsoft.EntityFrameworkCore;
using DataPacketLibrary;
using DataPacketLibrary.Models;
using RestrictRService;
using System.Security.Principal;
using Microsoft.Extensions.FileProviders;
using Windows.ApplicationModel.Activation;

//ApplicationBlocker applicationBlocker = new();
//WebsiteBlocker websiteBlocker = new();
//BlockingScheduler blockingScheduler = new(applicationBlocker, websiteBlocker);
//PipeCommunication pipe = new(blockingScheduler);

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "RestrictR Blocking Service";
    })
    //.ConfigureAppConfiguration((hostingContext, config) => {
    //    config.SetBasePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    //    config.AddJsonFile("myconfig.json", optional: false, reloadOnChange: true);
    //})
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
