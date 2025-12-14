using DutchMetar.Web.Client.Services.Interfaces;
using DutchMetar.Web.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAirportService, AirportService>();

await builder.Build().RunAsync();
