using System.Net.Http.Json;
using DutchMetar.Web.Client.Services.Interfaces;
using DutchMetar.Web.Shared.Constants;
using DutchMetar.Web.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace DutchMetar.Web.Shared.Services;

public class AirportService : IAirportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly NavigationManager _navigationManager;

    public AirportService(IHttpClientFactory httpClientFactory, NavigationManager navigationManager)
    {
        _httpClientFactory = httpClientFactory;
        _navigationManager = navigationManager;
    }

    public async Task<ICollection<AirportSummary>> GetAirportSummariesAsync()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_navigationManager.BaseUri);

        var httpResult = await client.GetAsync(EndpointConstants.AirportSummariesEndpoint);
        httpResult.EnsureSuccessStatusCode();
        
        var airports = await httpResult.Content.ReadFromJsonAsync<AirportSummary[]>();

        return airports ?? throw new NullReferenceException("Failed to deserialize JSON");
    }
}