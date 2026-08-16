using System.Text.RegularExpressions;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;

public class RawTafFileParser : IRawTafFileParser
{
    public Domain.Entities.Taf ParseRawTafToEntity(string rawTaf)
    {
        if (string.IsNullOrWhiteSpace(rawTaf))
            throw new TafParsingException("Raw TAF content cannot be null or empty.");

        var normalizedTaf = ExtractTafPayload(rawTaf);
        var match = Regex.Match(normalizedTaf,
            @"\bTAF(?:\s+(?:AMD|COR))?\s+([A-Z]{4})\s+(\d{6}Z)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        if (!match.Success)
            throw new TafParsingException("Unexpected TAF payload format.");

        var icao = match.Groups[1].Value.Trim();
        var issuedAtText = match.Groups[2].Value.Trim();

        if (issuedAtText.Length < 7 || !issuedAtText.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
            throw new TafParsingException("Unable to parse TAF issue time.");

        var issuedAt = ParseIssuedAt(issuedAtText);

        return new Domain.Entities.Taf
        {
            RawTaf = normalizedTaf,
            IssuedAt = issuedAt,
            AirportId = 0,
            Airport = new Domain.Entities.Airport { Icao = icao }
        };
    }

    private static DateTimeOffset? ParseIssuedAt(string issuedAtText)
    {
        if (issuedAtText.Length < 7 || !issuedAtText.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            var day = int.Parse(issuedAtText[..2]);
            var hour = int.Parse(issuedAtText.Substring(2, 2));
            var minute = int.Parse(issuedAtText.Substring(4, 2));

            var reference = DateTimeOffset.UtcNow;
            return new DateTimeOffset(reference.Year, reference.Month, day, hour, minute, 0, TimeSpan.Zero);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }

    private static string ExtractTafPayload(string rawTaf)
    {
        var lines = rawTaf
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var tafLineIndex = Array.FindIndex(lines, line => line.StartsWith("TAF", StringComparison.OrdinalIgnoreCase));

        if (tafLineIndex < 0)
            throw new TafParsingException("No TAF payload found in the provided content.");

        return string.Join(" ", lines[tafLineIndex..]);
    }
}