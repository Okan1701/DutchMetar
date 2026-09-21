using DutchMetar.Core.Features.Web.AirportDetails;
using DutchMetar.Core.Features.Web.AirportPerDayHistory;
using DutchMetar.Core.Features.Web.AirportSummary;
using DutchMetar.Core.Features.Web.MetarHistory;
using DutchMetar.Core.Infrastructure;
using DutchMetar.Core.Infrastructure.Data;
using DutchMetar.Web.Server.Constants;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DutchMetarContext>();
builder.Services.AddDutchMetarDatabaseContext(builder.Configuration);
builder.Services.AddAirportSummaryFeature();
builder.Services.AddAirportDetailsFeature();
builder.Services.AddAirportDayHistoryFeature();
builder.Services.AddMetarHistoryFeature();

var app = builder.Build();

app.UseDefaultFiles();
app.MapStaticAssets();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks(EndpointConstants.HealthEndpoint);
app.MapFallbackToFile("/index.html");

app.Run();
