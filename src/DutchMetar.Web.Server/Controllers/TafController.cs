using System.ComponentModel.DataAnnotations;
using DutchMetar.Core.Features.Web.TafHistory;
using DutchMetar.Web.Server.Constants;
using Microsoft.AspNetCore.Mvc;

namespace DutchMetar.Web.Server.Controllers;

[ApiController]
[Route(EndpointConstants.TafEndpoint)]
public class TafController : ControllerBase
{
    private readonly IGetTafHistoryFeature _getTafHistoryFeature;

    public TafController(IGetTafHistoryFeature getTafHistoryFeature)
    {
        _getTafHistoryFeature = getTafHistoryFeature;
    }

    [HttpGet("{airportIcao}")]
    public async Task<IActionResult> Get(
        [FromRoute] string airportIcao,
        [FromQuery] [Required] uint page,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        CancellationToken cancellationToken)
    {
        var request = new GetTafHistoryRequest
        {
            Icao = airportIcao,
            StartDate = startDate,
            EndDate = endDate,
            Page = (int)page
        };

        var data = await _getTafHistoryFeature.GetHistoryAsync(request, cancellationToken);
        return Ok(data);
    }
}
