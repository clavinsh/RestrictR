using RestrictRService;

ApplicationBlocker applicationBlocker = new();
PipeCommunication pipe = new(applicationBlocker);

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "RestrictR Blocking Service";
    })
    //.ConfigureAppConfiguration((hostingContext, config) => {
    //    config.SetBasePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    //    config.AddJsonFile("myconfig.json", optional: false, reloadOnChange: true);
    //})
    .ConfigureServices(services =>
    {
        services.AddSingleton(applicationBlocker);
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
