using System.Net.Http.Json;
using DutchMetar.Web.Shared.Constants;
using DutchMetar.Web.Shared.Models;
using DutchMetar.Web.Shared.Services.Interfaces;
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

        var httpResult = await client.GetAsync(EndpointConstants.AirportEndpoint);
        httpResult.EnsureSuccessStatusCode();
        
        var airports = await httpResult.Content.ReadFromJsonAsync<AirportSummary[]>();

        return airports ?? throw new NullReferenceException("Failed to deserialize JSON");
    }
    
    public async Task<AirportDetails> GetAirportDetailsAsync(string airportIcaoCode)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_navigationManager.BaseUri);

        var httpResult = await client.GetAsync(EndpointConstants.AirportEndpoint + airportIcaoCode);
        httpResult.EnsureSuccessStatusCode();
        
        var airportDetails = await httpResult.Content.ReadFromJsonAsync<AirportDetails>();

        return airportDetails ?? throw new NullReferenceException("Failed to deserialize JSON");
    }
    
    public async Task<AirportDayHistory> GetAirportHistoryAsync(string airportIcaoCode, DateOnly date)
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(_navigationManager.BaseUri);

        var url = $"{EndpointConstants.AirportEndpoint}{airportIcaoCode}/history?targetDate={date:yyyy-MM-dd}";
        var httpResult = await client.GetAsync(url);
        httpResult.EnsureSuccessStatusCode();
        
        var airportDayHistory= await httpResult.Content.ReadFromJsonAsync<AirportDayHistory>();

        return airportDayHistory ?? throw new NullReferenceException("Failed to deserialize JSON");
    }
}