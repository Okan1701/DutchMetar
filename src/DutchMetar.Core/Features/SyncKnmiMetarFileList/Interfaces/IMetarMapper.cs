using DutchMetar.Core.Domain.Entities;

namespace DutchMetar.Core.Features.SyncKnmiMetarFileList.Interfaces;

public interface IMetarMapper
{
    /// <summary>
    /// Maps a decoded <see cref="MetarParserCore.Objects.Metar"/> into an <see cref="Metar"/> entity
    /// </summary>
    Metar MapDecodedMetarToEntity(MetarParserCore.Objects.Metar decodedMetar, string rawMetar, Airport? airport = null,
        Guid correlationId = default);
}