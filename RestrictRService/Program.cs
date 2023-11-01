using RestrictRService;

IHost host = Host.CreateDefaultBuilder(args)
    //.ConfigureAppConfiguration((hostingContext, config) => {
    //    config.SetBasePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
    //    config.AddJsonFile("myconfig.json", optional: false, reloadOnChange: true);
    //})
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    })
    .Build();

PipeCommunication pipeCommunication = new();

//PipeClient pipeClient = new PipeClient();

await host.RunAsync();
