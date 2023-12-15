using Microsoft.EntityFrameworkCore;
using RestrictRService;
using RestrictRService.Models;

ApplicationBlocker applicationBlocker = new();
WebsiteBlocker websiteBlocker = new();
BlockingScheduler blockingScheduler = new(applicationBlocker, websiteBlocker);
PipeCommunication pipe = new(blockingScheduler);

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
        string? connectionString = hostContext.Configuration.GetConnectionString("Database");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        services.AddDbContext<RestrictRDbContext>(options => options.UseSqlite(connectionString));
        services.AddSingleton(applicationBlocker);
        services.AddSingleton(websiteBlocker);
        services.AddSingleton(blockingScheduler);
        services.AddSingleton(pipe);

        //services.AddSingleton(provider =>
        //{
        //    var applicationBlocker = provider.GetRequiredService<ApplicationBlocker>();
        //    return new PipeCommunication(applicationBlocker);
        //});
        services.AddHostedService<Worker>();
    })
    .Build();

//PipeClient pipeClient = new PipeClient();

await host.RunAsync();
