using DutchMetar.Core.Helpers.Enums;

namespace DutchMetar.Core.Features.Web.AirportDetails.Models;

public class AirportDetails
{
    public required string Icao { get; set; }

    public MetarMeteoCondition MeteoCondition { get; set; } = MetarMeteoCondition.None;

    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;
    
    public AirportCurrentMetar? LatestWeather { get; set; }
}