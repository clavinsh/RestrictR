using Serilog;

namespace RestrictRService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration; 
        private readonly IApplicationBlocker _appBlocker;
        private readonly IWebsiteBlocker _webBlocker;
        private readonly PipeCommunication _pipeCommunication;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, IApplicationBlocker appBlocker, IWebsiteBlocker webBlocker, PipeCommunication pipeCommunication)
        {
            _logger = logger;
            _configuration = configuration;
            _appBlocker = appBlocker;
            _webBlocker = webBlocker;
            _pipeCommunication = pipeCommunication;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("Service has succesfully started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                //_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                _appBlocker.ManageActiveProcesses();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
