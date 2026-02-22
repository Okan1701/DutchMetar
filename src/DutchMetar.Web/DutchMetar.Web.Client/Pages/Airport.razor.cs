using DutchMetar.Web.Client.Components.PageLayout;
using DutchMetar.Web.Shared.Models;
using DutchMetar.Web.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Pages;

public partial class Airport : ComponentBase
{
    private readonly IAirportService _airportService;
    private readonly ILogger<Airport> _logger;

    [Parameter]
    public required string AirportIcao { get; set; }
    
    private AirportDetails? AirportDetails { get; set; }
    
    private PageStatus PageStatus { get; set; }
    
    public Airport(IAirportService airportService, ILogger<Airport> logger)
    {
        _airportService = airportService;
        _logger = logger;
    }

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            PageStatus = PageStatus.Loading;
            AirportDetails = await _airportService.GetAirportDetailsAsync(AirportIcao);
            PageStatus = PageStatus.Displaying;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong when loading the page!");
            PageStatus = PageStatus.Error;
        }
    }

    private string FormatWindDirection()
    {
        if (AirportDetails?.LatestWeather is { WindSpeedKnots: not null, WindDirection: not null })
        {
            return $"{AirportDetails.LatestWeather.WindDirection} / {AirportDetails.LatestWeather.WindSpeedKnots} kt";
        }

        return "N/A";
    }
}