using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;

namespace DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;

public interface IMetarXmlMapper
{
    /// <summary>
    /// Maps a raw XML string from KNMI data platform to a Metar entity.
    /// The XML is expected to contain embedded raw METAR data in a comment and structured XML elements.
    /// </summary>
    /// <param name="xmlContent">Raw XML string that may contain leading whitespace and non-XML characters</param>
    /// <returns>Mapped Metar entity</returns>
    /// <exception cref="MetarMappingException">Thrown when mapping fails, including when xmlContent is null, empty, or invalid XML</exception>
    Metar Map(string xmlContent);
}
