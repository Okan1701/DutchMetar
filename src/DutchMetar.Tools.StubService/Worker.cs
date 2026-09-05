using DutchMetar.Tools.StubService.Services;

namespace DutchMetar.Tools.StubService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private const int IterationDelayMs = 1800000;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stub Worker started at: {time}", DateTimeOffset.Now);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMetarStubDataService>();

            try
            {
                await service.GenerateMetarReportForAllAirportsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred during worker iteration: {message}", ex.Message);
            }
            finally
            {
                _logger.LogInformation("Worker iteration finished at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(IterationDelayMs, stoppingToken);
        }
    }
}