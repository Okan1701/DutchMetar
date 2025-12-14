using DutchMetar.Core.Features.AirportSummary.Models;
using DutchMetar.Web.Shared.Models;

namespace DutchMetar.Web.Mapping;

public static class AirportSummaryMapping
{
    public static AirportSummary Map(SingleAirportSummary single)
    {
        return new AirportSummary
        {
            Icao = single.Icao,
            LastIssuedMetarDate = single.LastIssuedMetarDate,
            IsAuto = single.IsAuto,
            IsCavok = single.IsCavok,
            WindDirection = single.WindDirection,
            WindSpeedKnots = single.WindSpeedKnots
        };
    }

}