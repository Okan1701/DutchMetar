using DutchMetar.Web.Shared.Components.PageLayout;
using DutchMetar.Web.Shared.Models;
using DutchMetar.Web.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace DutchMetar.Web.Shared.Pages.Airport;

public partial class AirportPage : ComponentBase
{
    private readonly IAirportService _airportService;
    private readonly ILogger<AirportPage> _logger;

    [Parameter]
    public required string AirportIcao { get; set; }
    
    private AirportDetails? AirportDetails { get; set; }
    
    private PageStatus PageStatus { get; set; }
    
    public AirportPage(IAirportService airportService, ILogger<AirportPage> logger)
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
}