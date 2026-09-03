using DutchMetar.Core.Infrastructure;
using DutchMetar.Tools.StubService.Services;

namespace DutchMetar.Tools.StubService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddDutchMetarDatabaseContext(builder.Configuration);
        builder.Services.AddScoped<IMetarStubDataService, MetarStubDataService>();

        var host = builder.Build();
        host.Run();
    }
}