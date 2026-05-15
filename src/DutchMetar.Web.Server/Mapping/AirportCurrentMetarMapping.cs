using DutchMetar.Web.Server.Models;

namespace DutchMetar.Web.Server.Mapping;

public static class AirportCurrentMetarMapping
{
    public static AirportCurrentMetar Map(Core.Features.Web.AirportDetails.Models.AirportCurrentMetar source) => new()
    {
        RawMetar = source.RawMetar,
        IsAuto = source.IsAuto,
        IsCavok = source.IsCavok,
        IsCorrected = source.IsCorrected,
        WindDirection = source.WindDirection,
        WindSpeedKnots = source.WindSpeedKnots,
        WindSpeedGustsKnots = source.WindSpeedGustsKnots,
        NoCloudsDetected = source.NoCloudsDetected,
        VisibilityMeters = source.VisibilityMeters,
        TemperatureCelsius = source.TemperatureCelsius,
        DewpointCelsius = source.DewpointCelsius,
        AltimeterValue = source.AltimeterValue,
        Remarks = source.Remarks
    };
}