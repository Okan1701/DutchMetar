using DutchMetar.Web.Shared.Models;
using DutchMetar.Web.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Pages;

public partial class Airport : ComponentBase
{
    private readonly IAirportService _airportService;

    [Parameter]
    public required string AirportIcao { get; set; }
    
    private AirportDetails? AirportDetails { get; set; }

    private bool IsLoading { get; set; } = true;
    
    public Airport(IAirportService airportService)
    {
        _airportService = airportService;
    }

    protected override async Task OnParametersSetAsync()
    {
        IsLoading = true;
        AirportDetails = await _airportService.GetAirportDetailsAsync(AirportIcao);
        IsLoading = false;
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