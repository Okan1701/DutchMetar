using DutchMetar.Core.Features.AirportSummary.Interfaces;
using DutchMetar.Web.Mapping;
using DutchMetar.Web.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DutchMetar.Web.Controllers;

[ApiController]
public class AirportController : ControllerBase
{
    private readonly IGetAirportSummariesFeature _getAirportSummariesFeature;

    public AirportController(IGetAirportSummariesFeature getAirportSummariesFeature)
    {
        _getAirportSummariesFeature = getAirportSummariesFeature;
    }
    
    [HttpGet(EndpointConstants.AirportSummariesEndpoint)]
    public async Task<IActionResult> GetAirportSummary()
    {
        var airports = await _getAirportSummariesFeature.GetAirportSummariesAsync();
        return Ok(airports.Select(AirportSummaryMapping.Map));
    }
}