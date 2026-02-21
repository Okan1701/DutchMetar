using DutchMetar.Web.Shared.Services;
using DutchMetar.Web.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAirportService, AirportService>();
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
