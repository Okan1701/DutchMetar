using DutchMetar.Web.Shared.Models;
using DutchMetar.Web.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;

namespace DutchMetar.Web.Shared.Pages.Airport.Components;

public partial class AirportHistoricalData : ComponentBase
{
    private readonly IAirportService _airportService;
    private readonly ILogger<AirportHistoricalData> _logger;
    
    [Parameter]
    public required string Icao { get; set; }

    private DateTime? SelectedDate { get; set; } =  DateTime.Today;
    
    private bool IsLoading { get; set; } = true;
    
    private Exception? LoadingException { get; set; }
    
    private AirportDayHistory? History { get; set; }

    private void ResetSelectedDate()
    {
        SelectedDate = DateTime.Today;
    }

    public AirportHistoricalData(IAirportService airportService, ILogger<AirportHistoricalData> logger)
    {
        _airportService = airportService;
        _logger = logger;
    }

    protected override async Task OnInitializedAsync()
    {
        await RetrieveDataAsync();
    }

    private async Task RetrieveDataAsync()
    {
        IsLoading = true;
        LoadingException = null;

        try
        {
            History = await _airportService.GetAirportHistoryAsync(Icao, SelectedDate.ToDateOnly());
            IsLoading = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong when retrieving airport history");
            LoadingException = ex;
        }
    }
}