namespace DutchMetar.Tools.StubService.Services;

public interface IMetarStubDataService
{
    Task GenerateMetarReportForAllAirportsAsync(CancellationToken cancellationToken);
}