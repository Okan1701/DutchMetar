using DutchMetar.Core.Features.DataWarehouse;
using DutchMetar.Core.Features.DataWarehouse.Features.SyncKnmiMetar.Interfaces;
using DutchMetar.Core.Infrastructure;
using DutchMetar.Core.Infrastructure.Accessors;
using DutchMetar.Core.Infrastructure.Data;
using DutchMetar.Hangfire.Host;
using Hangfire;
using Microsoft.EntityFrameworkCore;

const string hangfireConnectionStringKey = "HangfireMssql";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ICorrelationIdAccessor, SimpleCorrelationIdAccessor>();
builder.Services.AddSyncKnmiMetarFileListFeature(builder.Configuration);
builder.Services.AddDutchMetarDatabaseContext(builder.Configuration);
builder.Services.AddHangfireServer();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
#if RELEASE
    .UseSqlServerStorage(builder.Configuration.GetConnectionString(hangfireConnectionStringKey)));
#else
    .UseInMemoryStorage());
#endif

var app = builder.Build();
app.UseHangfireDashboard("", new DashboardOptions
{
    AppPath = null,
    DarkModeEnabled = true,
    DashboardTitle = "DutchMetar - Hangfire",
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
GlobalJobFilters.Filters.Add(new DisableConcurrentExecutionAttribute(120));
RecurringJob.AddOrUpdate<ISyncKnmiMetarFileListFeature>("syncKnmiMetarFiles", feature => feature.SyncKnmiMetarFiles(CancellationToken.None),  Cron.HourInterval(1));
app.Run();