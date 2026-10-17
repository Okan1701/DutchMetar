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

    /// <summary>
    /// Maps the supplied raw TAF payload, resolving its UTC issue time to the nearest valid date
    /// in the reference month or either adjacent month.
    /// </summary>
    /// <param name="rawTaf">The raw TAF content received from KNMI.</param>
    /// <param name="referenceDate">The KNMI file creation date, expected to be near the issue time.</param>
    /// <returns>The parsed TAF entity.</returns>
    Domain.Entities.Taf ParseRawTafToEntity(string rawTaf, DateTimeOffset referenceDate);
}