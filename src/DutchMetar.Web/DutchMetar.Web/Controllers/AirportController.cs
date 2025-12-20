using DutchMetar.Core.Features.AIrportDetails.Interfaces;
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

    public AirportController(IGetAirportSummariesFeature getAirportSummariesFeature, IGetAirportDetailsFeature getAirportDetailsFeature)
    {
        _getAirportSummariesFeature = getAirportSummariesFeature;
        _getAirportDetailsFeature = getAirportDetailsFeature;
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
            CurrentWeather = AirportCurrentMetarMapping.Map(airportDetails.CurrentWeather)
        });
    }
}