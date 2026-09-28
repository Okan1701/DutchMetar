using DutchMetar.Core.Features.DataWarehouse.Features.Metar.DailySync;
using DutchMetar.Core.Features.DataWarehouse.Features.Taf.DailySync;
using Sentry.Hangfire;

namespace DutchMetar.WorkerService;

public sealed class ScheduledSyncJobs(
    IDailyMetarSyncFeature dailyMetarSyncFeature,
    IDailyTafSyncFeature dailyTafSyncFeature)
{
    [SentryMonitorSlug("knmi-daily-metar-sync")]
    public Task SyncKnmiMetarFiles(CancellationToken cancellationToken) =>
        dailyMetarSyncFeature.SyncKnmiMetarFiles(cancellationToken);

    [SentryMonitorSlug("knmi-daily-taf-sync")]
    public Task SyncKnmiTafFiles(CancellationToken cancellationToken) =>
        dailyTafSyncFeature.SyncKnmiTafFiles(cancellationToken);
}
