namespace RestrictRService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration; 
        private readonly ApplicationBlocker _appBlocker;

        public Worker(ILogger<Worker> logger, IConfiguration configuration, ApplicationBlocker appBlocker)
        {
            _logger = logger;
            _configuration = configuration;
            _appBlocker = appBlocker;
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