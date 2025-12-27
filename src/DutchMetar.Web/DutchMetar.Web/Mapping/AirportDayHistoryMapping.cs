using DutchMetar.Web.Shared.Models;

namespace DutchMetar.Web.Mapping;

public static class AirportDayHistoryMappingExtensions
{
    public static AirportDayHistory MapToWebModel(
        this Core.Features.AirportPerDayHistory.Models.AirportDayHistory source)
    {
        return new AirportDayHistory
        {
            Icao = source.Icao,
            IsMissingData = source.IsMissingData,
            History = source.History.Select(x => new AirportDayHistorySnapshot
            {
                DateTime = x.DateTime,
                DewpointCelsius = x.DewpointCelsius,
                TemperatureCelsius = x.TemperatureCelsius,
                VisibilityMeters = x.VisibilityMeters,
                WindSpeedKnots = x.WindSpeedKnots
            }).ToArray()
        };
    }
}