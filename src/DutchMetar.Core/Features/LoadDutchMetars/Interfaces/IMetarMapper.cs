using DutchMetar.Core.Domain.Entities;

namespace DutchMetar.Core.Features.LoadDutchMetars.Interfaces;

public interface IMetarMapper
{
    Metar MapDecodedMetarToEntity(MetarParserCore.Objects.Metar decodedMetar, string rawMetar, Airport? airport = null,
        Guid correlationId = default);
}