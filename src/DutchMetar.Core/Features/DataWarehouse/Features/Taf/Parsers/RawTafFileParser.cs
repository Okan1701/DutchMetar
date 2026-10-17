using System.Text.RegularExpressions;

namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;

public class RawTafFileParser : IRawTafFileParser
{
    public Domain.Entities.Taf ParseRawTafToEntity(string rawTaf)
    {
        return ParseRawTaf(rawTaf, null);
    }

    public Domain.Entities.Taf ParseRawTafToEntity(string rawTaf, DateTimeOffset referenceDate)
    {
        return ParseRawTaf(rawTaf, referenceDate);
    }

    private static Domain.Entities.Taf ParseRawTaf(string rawTaf, DateTimeOffset? referenceDate)
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

        var issuedAt = ParseIssuedAt(issuedAtText, referenceDate);

        return new Domain.Entities.Taf
        {
            RawTaf = normalizedTaf,
            IssuedAt = issuedAt,
            AirportId = 0,
            Airport = new Domain.Entities.Airport { Icao = icao }
        };
    }

    private static DateTimeOffset? ParseIssuedAt(string issuedAtText, DateTimeOffset? referenceDate)
    {
        if (issuedAtText.Length < 7 || !issuedAtText.EndsWith("Z", StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            var day = int.Parse(issuedAtText[..2]);
            var hour = int.Parse(issuedAtText.Substring(2, 2));
            var minute = int.Parse(issuedAtText.Substring(4, 2));

            if (referenceDate is null)
            {
                var now = DateTimeOffset.UtcNow;
                return new DateTimeOffset(now.Year, now.Month, day, hour, minute, 0, TimeSpan.Zero);
            }

            if (day is < 1 or > 31 || hour is < 0 or > 23 || minute is < 0 or > 59)
                return null;

            var reference = referenceDate.Value.ToUniversalTime();
            var month = new DateTimeOffset(reference.Year, reference.Month, 1, 0, 0, 0, TimeSpan.Zero);
            DateTimeOffset? closest = null;

            // A file published just after a month boundary may contain a TAF from the previous month.
            foreach (var offset in new[] { 0, -1, 1 })
            {
                if ((offset == -1 && month.Year == 1 && month.Month == 1) ||
                    (offset == 1 && month.Year == 9999 && month.Month == 12))
                    continue;

                var candidateMonth = month.AddMonths(offset);
                if (day > DateTime.DaysInMonth(candidateMonth.Year, candidateMonth.Month))
                    continue;

                var candidate = new DateTimeOffset(candidateMonth.Year, candidateMonth.Month, day, hour, minute, 0,
                    TimeSpan.Zero);
                if (closest is null || (candidate - reference).Duration() < (closest.Value - reference).Duration())
                    closest = candidate;
            }

            return closest;
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