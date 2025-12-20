using DutchMetar.Web.Shared.Services;
using DutchMetar.Web.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAirportService, AirportService>();

await builder.Build().RunAsync();
