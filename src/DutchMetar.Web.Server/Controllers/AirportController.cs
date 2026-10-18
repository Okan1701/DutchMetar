using DutchMetar.Core.Features.Web.AirportPerDayHistory.Interfaces;
using DutchMetar.Core.Features.Web.AirportSummary.Interfaces;
using DutchMetar.Web.Server.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DutchMetar.Web.Server.Controllers;

[ApiController]
[Route(EndpointConstants.AirportEndpoint)]
public class AirportController : ControllerBase
{
    private readonly IGetAirportSummariesFeature _getAirportSummariesFeature;
    private readonly IGetAirportDayHistoryFeature _getAirportDayHistoryFeature;

    public AirportController(IGetAirportSummariesFeature getAirportSummariesFeature, IGetAirportDayHistoryFeature getAirportDayHistoryFeature)
    {
        _getAirportSummariesFeature = getAirportSummariesFeature;
        _getAirportDayHistoryFeature = getAirportDayHistoryFeature;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAirportSummaries()
    {
        var airports = await _getAirportSummariesFeature.GetAirportSummariesAsync();
        return Ok(airports);
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

        return Ok(result);
    }
}