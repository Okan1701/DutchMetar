using DutchMetar.Core.Infrastructure.Data;
using DutchMetar.Tools.StubData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Stub data generator has started. Quit via Ctrl + C!");

// Handle SIGINT event
var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (sender, eventArgs) =>
{
    cancellationTokenSource.Cancel();
};

// Setup appsettings
var builder = new ConfigurationBuilder()
    .AddJsonFile($"appsettings.json", true, false)
    .AddJsonFile($"appsettings.Development.json", true, false)
    .AddEnvironmentVariables();
var configuration = builder.Build();

// Setup EF Core
var contextOptions = new DbContextOptionsBuilder<DutchMetarContext>();
contextOptions.UseSqlServer(configuration.GetConnectionString("DutchMetarMssql"));
var context = new DutchMetarContext(contextOptions.Options);

// Start generator
var generator = new StubDataGenerator(context);
generator.GeneratePeriodicMetarsAsync(cancellationTokenSource.Token).Wait();

