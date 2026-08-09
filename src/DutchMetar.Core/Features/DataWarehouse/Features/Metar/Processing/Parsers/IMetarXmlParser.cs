using DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Exceptions;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Metar.Processing.Parsers;

public interface IMetarXmlParser
{
    /// <summary>
    /// Parses a raw XML string from KNMI data platform to a Metar entity.
    /// The XML is expected to contain embedded raw METAR data in a comment and structured XML elements.
    /// </summary>
    /// <param name="xmlContent">Raw XML string that may contain leading whitespace and non-XML characters</param>
    /// <returns>Mapped Metar entity</returns>
    /// <exception cref="MetarXmlParsingException">Thrown when parsing fails, including when xmlContent is null, empty, or invalid XML</exception>
    Domain.Entities.Metar Map(string xmlContent);
}
