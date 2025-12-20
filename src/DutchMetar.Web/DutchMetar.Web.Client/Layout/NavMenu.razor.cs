using DutchMetar.Web.Shared.Models;
using DutchMetar.Web.Shared.Services.Interfaces;
using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Client.Layout;

public partial class NavMenu : ComponentBase
{
    private readonly IAirportService _airportService;
    private readonly NavigationManager _navigationManager;

    private bool IsLoading { get; set; } = true;
    
    private ICollection<AirportSummary> AirportSummaries { get; set; } = [];

    public NavMenu(IAirportService airportService, NavigationManager navigationManager)
    {
        _airportService = airportService;
        _navigationManager = navigationManager;
    }

    protected override async Task OnInitializedAsync()
    {
        AirportSummaries = await _airportService.GetAirportSummariesAsync();
        IsLoading = false;
    }

    private void OnAirportSelected(string icao)
    {
        _navigationManager.NavigateTo($"/airport/{icao}");
    }
}