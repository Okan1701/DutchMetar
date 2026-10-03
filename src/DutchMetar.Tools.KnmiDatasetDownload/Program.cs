using DutchMetar.Core.Features.DataWarehouse;
using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Processing.Handlers;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Contracts;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.Repositories.Interfaces;
using DutchMetar.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

const int fileDownloadIntervalMs = 1000;

if (args.Length > 1)
{
    Console.Error.WriteLine($"Specify only one dataset: {KnmiDatasetNames.Metar} or {KnmiDatasetNames.Taf}.");
    return 1;
}

var datasetInput = args.Length == 1 ? args[0] : PromptForDataset();
var dataset = datasetInput.Trim().ToLowerInvariant() switch
{
    KnmiDatasetNames.Metar => KnmiDatasetNames.Metar,
    KnmiDatasetNames.Taf => KnmiDatasetNames.Taf,
    _ => null
};

if (dataset == null)
{
    Console.Error.WriteLine("Dataset must be either metar or taf.");
    return 1;
}

var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? Environments.Development;
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = [],
    ContentRootPath = AppContext.BaseDirectory,
    EnvironmentName = environmentName
});
builder.Services.AddDataWarehouseServices(builder.Configuration);
builder.Services.AddDutchMetarDatabaseContext(builder.Configuration);

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("KnmiDatasetDownload");
var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};;

try
{
    ICollection<KnmiFileMeta> files;
    using (var scope = host.Services.CreateScope())
    {
        var repository = scope.ServiceProvider.GetRequiredService<IKnmiRepository>();
        var parameters = new KnmiFilesParameters
        {
            Sorting = "asc",
            OrderBy = "created"
        };

        files = dataset == KnmiDatasetNames.Metar
            ? await repository.GetKnmiMetarFiles(parameters, cancellationTokenSource.Token)
            : await repository.GetKnmiTafFiles(parameters, cancellationTokenSource.Token);
    }

    logger.LogInformation("Found {FileCount} KNMI {Dataset} files.", files.Count, dataset);

    for (var index = 0; index < files.Count; index++)
    {
        cancellationTokenSource.Token.ThrowIfCancellationRequested();
        var file = files.ElementAt(index);

        using var scope = host.Services.CreateScope();
        if (dataset == KnmiDatasetNames.Metar)
        {
            var handler = scope.ServiceProvider.GetRequiredService<IMetarFileHandler>();
            await handler.HandleFileAsync(file, cancellationTokenSource.Token);
        }
        else
        {
            var handler = scope.ServiceProvider.GetRequiredService<ITafFileHandler>();
            await handler.HandleFileAsync(file, cancellationTokenSource.Token);
        }

        logger.LogInformation("Processed {Dataset} file {FileNumber} of {FileCount}: {FileName}",
            dataset, index + 1, files.Count, file.FileName);
        await Task.Delay(fileDownloadIntervalMs, cancellationTokenSource.Token);
    }

    logger.LogInformation("Finished downloading the KNMI {Dataset} dataset.", dataset);
}
catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
{
    logger.LogInformation("KNMI {Dataset} dataset download was cancelled.", dataset);
}

return 0;

static string PromptForDataset()
{
    Console.Write("Which KNMI dataset should be downloaded (metar/taf)? ");
    return Console.ReadLine() ?? string.Empty;
}