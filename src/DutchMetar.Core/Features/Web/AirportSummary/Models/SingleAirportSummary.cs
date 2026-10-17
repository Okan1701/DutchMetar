using DutchMetar.Core.Helpers.Enums;
using DutchMetar.Core.Features.Web.AirportDetails.Models;

namespace DutchMetar.Core.Features.Web.AirportSummary.Models;

public class SingleAirportSummary
{
    public required string Icao  { get; set; }

    public string? Name { get; set; }

    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.MinValue;

    public AirportCurrentMetar? LatestWeather { get; set; }

    public AirportForecast? LatestForecast { get; set; }
    
    public DateTimeOffset LastIssuedMetarDate { get; set; }
    
    public bool IsAuto { get; set; }
    
    public bool IsCavok { get; set; }
    
    public bool IsCorrected { get; set; }

    public MetarMeteoCondition MeteoCondition { get; set; } = MetarMeteoCondition.None;
    
    public int? WindDirection { get; set; }
    
    public int? WindSpeedKnots { get; set; }
}