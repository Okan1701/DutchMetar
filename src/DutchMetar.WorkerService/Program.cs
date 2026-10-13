using System.Diagnostics;
using System.Reflection;
using DutchMetar.Core.Features.DataWarehouse;
using DutchMetar.Core.Features.DataWarehouse.Infrastructure.HostedServices;
using DutchMetar.Core.Infrastructure;
using DutchMetar.Core.Infrastructure.Data;
using DutchMetar.WorkerService;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Sentry.Hangfire;

const string sentryDsnConnectionString = "SentryDsn";
#if RELEASE
const string hangfireConnectionStringKey = "HangfireMssql";
#endif

// Get application version
var assembly = Assembly.GetExecutingAssembly();
var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
var version = fileVersionInfo?.ProductVersion;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration.GetConnectionString(sentryDsnConnectionString);
    // Set TracesSampleRate to 1.0 to capture 100%
    // of transactions for tracing.
    // We recommend adjusting this value in production
    o.TracesSampleRate = 1.0;
    // Enable logs to be sent to Sentry
    o.EnableLogs = true;
});

builder.Host.UseWindowsService();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DutchMetarContext>();
builder.Services.AddDataWarehouseServices(builder.Configuration);
builder.Services.AddDutchMetarDatabaseContext(builder.Configuration);
builder.Services.AddTransient<ScheduledSyncJobs>();
builder.Services.AddHangfireServer();
builder.Services.AddHostedService<NotificationHostedService>();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSentry()
#if RELEASE
    .UseSqlServerStorage(builder.Configuration.GetConnectionString(hangfireConnectionStringKey)));
#else
    .UseInMemoryStorage());
#endif

var app = builder.Build();
app.UseRouting();
app.MapHealthChecks("/health").ShortCircuit();
app.UseHangfireDashboard("", new DashboardOptions
{
    AppPath = null,
    DarkModeEnabled = true,
    DashboardTitle = string.IsNullOrEmpty(version) ? "DutchMetar Worker" : $"DutchMetar Worker v{version.Split('+')[0]}",
    DisplayStorageConnectionString = true,
    Authorization = [new HangfireAuthorizationFilter()]
});

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DutchMetarContext>();
    context.Database.Migrate();
}


// Register recurring jobs
GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Fail});
GlobalJobFilters.Filters.Add(new DisableConcurrentExecutionAttribute(3600));
RecurringJob.AddOrUpdate<ScheduledSyncJobs>("KnmiDailySync", job => job.SyncKnmiMetarFiles(CancellationToken.None), Cron.DayInterval(1));
RecurringJob.AddOrUpdate<ScheduledSyncJobs>("KnmiDailyTafSync", job => job.SyncKnmiTafFiles(CancellationToken.None), Cron.DayInterval(1));

app.Run();