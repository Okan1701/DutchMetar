namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;

/// <summary>
/// Parses raw KNMI TAF payloads into domain entities.
/// </summary>
public interface IRawTafFileParser
{
    /// <summary>
    /// Maps the supplied raw TAF payload to a populated TAF entity.
    /// </summary>
    /// <param name="rawTaf">The raw TAF content received from KNMI.</param>
    /// <returns>The parsed TAF entity.</returns>
    Domain.Entities.Taf ParseRawTafToEntity(string rawTaf);
}