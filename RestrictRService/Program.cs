using Microsoft.EntityFrameworkCore;
using DataPacketLibrary;
using DataPacketLibrary.Models;
using RestrictRService;

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
        services.AddSingleton<ApplicationBlocker>();
        services.AddSingleton<WebsiteBlocker>();
        services.AddScoped<EventController>();
        services.AddSingleton<BlockingScheduler>();
        services.AddSingleton<PipeCommunication>();
        services.AddHostedService<Worker>();
    })
    .Build();

// Apply pending database migrations - used for client database setup
using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RestrictRDbContext>();
    dbContext.Database.Migrate();
}
//PipeClient pipeClient = new PipeClient();

await host.RunAsync();
