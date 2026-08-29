using System.ComponentModel.DataAnnotations;
using DutchMetar.Core.Features.Web.MetarHistory;
using DutchMetar.Web.Server.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DutchMetar.Web.Server.Controllers;

[ApiController]
[Route(EndpointConstants.MetarEndpoint)]
public class MetarController : ControllerBase
{
    private readonly IGetMetarHistoryFeature _getMetarHistoryFeature;

    public MetarController(IGetMetarHistoryFeature getMetarHistoryFeature)
    {
        _getMetarHistoryFeature = getMetarHistoryFeature;
    }

    [HttpGet("{airportIcao}")]
    public async Task<IActionResult> Get(
        [FromRoute] string airportIcao, 
        [FromQuery] [Required] uint page,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken)
    {
        var request = new GetMetarHistoryRequest
        {
            Icao = airportIcao,
            StartDate = startDate,
            EndDate = endDate,
            Page = (int)page
        };

        var data = await _getMetarHistoryFeature.GetHistoryAsync(request, cancellationToken);
        return Ok(data);
    }
}