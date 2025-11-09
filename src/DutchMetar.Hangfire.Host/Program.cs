using DutchMetar.Core.Features.LoadDutchMetars;
using DutchMetar.Core.Features.LoadDutchMetars.Interfaces;
using DutchMetar.Core.Infrastructure;
using Hangfire;

const string hangfireConnectionStringKey = "HangfireMssql";

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLoadDutchMetarsFeature();
builder.Services.AddDutchMetarDatabaseContext(builder.Configuration);
builder.Services.AddHangfireServer();
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString(hangfireConnectionStringKey)));

var app = builder.Build();
app.UseHangfireDashboard("", new DashboardOptions
{
    AppPath = null,
    DarkModeEnabled = true,
    DashboardTitle = "DutchMetar - Hangfire",
    DisplayStorageConnectionString = true
});

// Register recurring jobs
RecurringJob.AddOrUpdate<ILoadDutchMetarsFeature>("loadMetar", feature => feature.LoadAsync(CancellationToken.None),  Cron.MinuteInterval(10));

app.Run();