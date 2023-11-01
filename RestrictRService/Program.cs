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

PipeClient pipeClient = new PipeClient();

await host.RunAsync();
