using DutchMetar.Core.Features.AIrportDetails.Interfaces;
using DutchMetar.Core.Features.AirportPerDayHistory.Interfaces;
using DutchMetar.Core.Features.AirportSummary.Interfaces;
using DutchMetar.Web.Mapping;
using DutchMetar.Web.Shared.Constants;
using DutchMetar.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace DutchMetar.Web.Controllers;

[ApiController]
[Route(EndpointConstants.AirportEndpoint)]
public class AirportController : ControllerBase
{
    private readonly IGetAirportSummariesFeature _getAirportSummariesFeature;
    private readonly IGetAirportDetailsFeature _getAirportDetailsFeature;
    private readonly IGetAirportDayHistoryFeature _getAirportDayHistoryFeature;

    public AirportController(IGetAirportSummariesFeature getAirportSummariesFeature, IGetAirportDetailsFeature getAirportDetailsFeature, IGetAirportDayHistoryFeature getAirportDayHistoryFeature)
    {
        _getAirportSummariesFeature = getAirportSummariesFeature;
        _getAirportDetailsFeature = getAirportDetailsFeature;
        _getAirportDayHistoryFeature = getAirportDayHistoryFeature;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAirportSummaries()
    {
        var airports = await _getAirportSummariesFeature.GetAirportSummariesAsync();
        return Ok(airports.Select(AirportSummaryMapping.Map));
    }
    
    [HttpGet("{airportIcao}")]
    public async Task<IActionResult> GetAirportDetails([FromRoute] string airportIcao, CancellationToken cancellationToken)
    {
        var airportDetails = await _getAirportDetailsFeature.GetAirportDetailsAsync(airportIcao, cancellationToken);
        return Ok(new AirportDetails
        {
            Icao =  airportDetails.Icao,
            LastUpdated =  airportDetails.LastUpdated,
            CurrentWeather = airportDetails.CurrentWeather != null ? AirportCurrentMetarMapping.Map(airportDetails.CurrentWeather) : null,
            HistoricalWeather = airportDetails.HistoricalWeather.Select(AirportCurrentMetarMapping.Map).ToArray()
        });
    }

    [HttpGet("{airportIcao}/history")]
    public async Task<IActionResult> GetAirportHistory([FromRoute] string airportIcao, [FromQuery] DateOnly? targetDate,
        CancellationToken cancellationToken)
    {
        targetDate ??= DateOnly.FromDateTime(DateTime.Today);
        var result = await _getAirportDayHistoryFeature.GetAirportDayHistoryAsync(new()
        {
            Date = targetDate.Value,
            Icao = airportIcao
        }, cancellationToken);

        return Ok(result.MapToWebModel());
    }
}