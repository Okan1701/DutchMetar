using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Features.SyncKnmiMetar.Interfaces;
using MetarParserCore.Enums;

namespace DutchMetar.Core.Features.SyncKnmiMetar.Services;

public class MetarMapper : IMetarMapper
{
    private static readonly CloudType[] SourceCloudTypes =
    [
        CloudType.Few,
        CloudType.Scattered,
        CloudType.Broken,
        CloudType.Overcast
    ];

    public Metar MapDecodedMetarToEntity(
        MetarParserCore.Objects.Metar decodedMetar, 
        string rawMetar,
        DateTimeOffset createdAt,
        Airport? airport = null,
        Guid correlationId = default)
    {
        if (decodedMetar?.ObservationDayTime == null)
        {
            throw new NullReferenceException("Source METAR observation could not be parsed");
        }

        var sourceLayers = decodedMetar.CloudLayers ?? [];
        var ceilings = sourceLayers
            .Where(x => SourceCloudTypes.Contains(x.CloudType))
            .Select(x => new MetarCeiling
            {
                Type = MapSourceCloudType(x.CloudType),
                Height = x.Altitude,
            });


        return new()
        {
            CorrelationId = correlationId,
            IsAuto = decodedMetar.Modifier == MetarModifier.Auto,
            IsCorrected = decodedMetar.Modifier == MetarModifier.Cor || rawMetar.Contains(" COR "),
            Airport = airport,
            RawMetar = rawMetar,
            WindDirection = decodedMetar?.SurfaceWind?.Direction,
            WindSpeedKnots = decodedMetar?.SurfaceWind?.Speed,
            WindSpeedGustsKnots = decodedMetar?.SurfaceWind?.GustSpeed > 0 ? decodedMetar.SurfaceWind.GustSpeed : null,
            AltimeterValue = decodedMetar?.AltimeterSetting?.Value,
            TemperatureCelsius = decodedMetar?.Temperature?.Value,
            DewpointCelsius = decodedMetar?.Temperature?.DewPoint,
            IsCavok = decodedMetar?.PrevailingVisibility?.IsCavok ?? false,
            NoCloudsDetected = sourceLayers.Any(x => x.CloudType == CloudType.NoCloudDetected),
            VisibilityMeters = decodedMetar?.PrevailingVisibility?.VisibilityInMeters?.VisibilityValue,
            Remarks = decodedMetar?.Remarks,
            Ceilings = ceilings.ToArray(),
            IssuedAt = new DateTime(createdAt.Year, createdAt.Month, decodedMetar!.ObservationDayTime.Day,
                decodedMetar.ObservationDayTime.Time.Hours, decodedMetar.ObservationDayTime.Time.Minutes, 0,
                DateTimeKind.Utc)
        };
    }

    private CeilingType MapSourceCloudType(CloudType cloudType) => cloudType switch
    {
        CloudType.Few => CeilingType.Few,
        CloudType.Scattered => CeilingType.Scattered,
        CloudType.Broken => CeilingType.Broken,
        CloudType.Overcast => CeilingType.Overcast,
        _ => CeilingType.Other
    };
}