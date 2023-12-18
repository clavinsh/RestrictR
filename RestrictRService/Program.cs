using Microsoft.EntityFrameworkCore;
using DataPacketLibrary;
using DataPacketLibrary.Models;
using RestrictRService;
using System.Security.Principal;
using Microsoft.Extensions.FileProviders;

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

// essentially used for first time setup - creates the database
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RestrictRDbContext>();
    dbContext.Database.EnsureCreated();
}

await host.RunAsync();
