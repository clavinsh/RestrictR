namespace RestrictRService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration; 
        private readonly ApplicationBlocker _appBlocker;
        private readonly WebsiteBlocker _webBlocker;
        private readonly PipeCommunication _pipeCommunication;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, ApplicationBlocker appBlocker, WebsiteBlocker webBlocker, PipeCommunication pipeCommunication)
        {
            _logger = logger;
            _configuration = configuration;
            _appBlocker = appBlocker;
            _webBlocker = webBlocker;
            _pipeCommunication = pipeCommunication;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                var configValue = _configuration["someKey"];

                _appBlocker.ManageActiveProcesses();

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
