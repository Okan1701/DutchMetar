using System.Xml.Linq;
using System.Text.RegularExpressions;
using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;

namespace DutchMetar.Core.Features.DataWarehouse.Shared;

public class MetarXmlMapper : IMetarXmlMapper
{
    public Metar Map(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
            throw new MetarMappingException("XML content cannot be null or empty.");

        try
        {
            var cleanedXml = CleanXmlContent(xmlContent);
            var document = XDocument.Parse(cleanedXml);
            var root = document.Root;

            if (root == null)
                throw new MetarMappingException("XML document has no root element.");

            var ns = new Dictionary<string, XNamespace>
            {
                { "iwxxm", XNamespace.Get("http://icao.int/iwxxm/3.0") },
                { "gml", XNamespace.Get("http://www.opengis.net/gml/3.2") },
                { "aixm", XNamespace.Get("http://www.aixm.aero/schema/5.1.1") }
            };

            var metar = new Metar
            {
                RawMetar = ExtractRawMetarFromComment(document) ?? string.Empty,
                IssuedAt = ExtractIssueTime(root, ns) ?? DateTimeOffset.UtcNow,
                IsAuto = ExtractIsAuto(root),
                IsCavok = ExtractIsCavok(root, ns),
                IsCorrected = ExtractIsCorrected(root),
                WindDirection = ExtractWindDirection(root, ns),
                WindSpeedKnots = ExtractWindSpeed(root, ns),
                WindSpeedGustsKnots = ExtractWindGust(root, ns),
                VisibilityMeters = ExtractVisibility(root, ns),
                TemperatureCelsius = ExtractTemperature(root, ns),
                DewpointCelsius = ExtractDewpoint(root, ns),
                AltimeterValue = ExtractAltimeter(root, ns),
                Remarks = ExtractRemarks(root, ns),
                NoCloudsDetected = ExtractNoCloudsDetected(root, ns)
            };

            return metar;
        }
        catch (Exception ex)
        {
            throw new MetarMappingException("Failed to map METAR XML content.", ex);
        }
    }

    private string CleanXmlContent(string xmlContent)
    {
        var xmlStartIndex = xmlContent.IndexOf("<?xml", StringComparison.OrdinalIgnoreCase);

        if (xmlStartIndex < 0)
            xmlStartIndex = xmlContent.IndexOf('<');

        if (xmlStartIndex < 0)
            return xmlContent;

        return xmlContent.Substring(xmlStartIndex).Trim();
    }

    private string? ExtractRawMetarFromComment(XDocument document)
    {
        var comment = document.Nodes()
            .OfType<XComment>()
            .FirstOrDefault();

        if (comment == null && document.Root != null)
        {
            comment = document.Root.Nodes()
                .OfType<XComment>()
                .FirstOrDefault();
        }

        var commentValue = comment?.Value?.Trim();

        if (string.IsNullOrWhiteSpace(commentValue))
            return null;

        return commentValue;
    }

    private DateTimeOffset? ExtractIssueTime(XElement root, Dictionary<string, XNamespace> ns)
    {
        var issueTimeElement = root.Element(ns["iwxxm"] + "issueTime");
        var timePosition = issueTimeElement?
            .Descendants(ns["gml"] + "timePosition")
            .FirstOrDefault()?
            .Value;

        if (DateTimeOffset.TryParse(timePosition, null, System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var result))
            return result;

        return null;
    }

    private bool ExtractIsAuto(XElement root)
    {
        var automatedStation = root.Attribute("automatedStation")?.Value;
        return automatedStation?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private bool ExtractIsCavok(XElement root, Dictionary<string, XNamespace> ns)
    {
        var observation = root.Element(ns["iwxxm"] + "observation");
        var meteorologicalObs = observation?.Element(ns["iwxxm"] + "MeteorologicalAerodromeObservation");
        var cavokAttr = meteorologicalObs?.Attribute("cloudAndVisibilityOK")?.Value;

        return cavokAttr?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private bool ExtractNoCloudsDetected(XElement root, Dictionary<string, XNamespace> ns)
    {
        var observation = root.Element(ns["iwxxm"] + "observation");
        var meteorologicalObs = observation?.Element(ns["iwxxm"] + "MeteorologicalAerodromeObservation");

        // Return true if cloud element is missing, indicating no clouds detected
        var cloudElement = meteorologicalObs?.Element(ns["iwxxm"] + "cloud");
        return cloudElement == null;
    }

    private bool ExtractIsCorrected(XElement root)
    {
        var reportStatus = root.Attribute("reportStatus")?.Value;
        return reportStatus?.Equals("CORRECTION", StringComparison.OrdinalIgnoreCase) ?? false;
    }

    private int? ExtractWindDirection(XElement root, Dictionary<string, XNamespace> ns)
    {
        var windDirection = root
            .Descendants(ns["iwxxm"] + "meanWindDirection")
            .FirstOrDefault()?
            .Value;

        if (decimal.TryParse(windDirection, System.Globalization.CultureInfo.InvariantCulture, out var direction))
            return (int)Math.Round(direction);

        return null;
    }

    private int? ExtractWindSpeed(XElement root, Dictionary<string, XNamespace> ns)
    {
        var windSpeed = root
            .Descendants(ns["iwxxm"] + "meanWindSpeed")
            .FirstOrDefault()?
            .Value;

        if (decimal.TryParse(windSpeed, System.Globalization.CultureInfo.InvariantCulture, out var speed))
            return (int)Math.Round(speed);

        return null;
    }

    private int? ExtractWindGust(XElement root, Dictionary<string, XNamespace> ns)
    {
        // Wind gust data is not explicitly in IWXXM structure; would require parsing raw METAR string
        return null;
    }

    private int? ExtractVisibility(XElement root, Dictionary<string, XNamespace> ns)
    {
        var visibility = root
            .Descendants(ns["iwxxm"] + "prevailingVisibility")
            .FirstOrDefault()?
            .Value;

        if (decimal.TryParse(visibility, System.Globalization.CultureInfo.InvariantCulture, out var vis))
            return (int)Math.Round(vis);

        return null;
    }

    private int? ExtractTemperature(XElement root, Dictionary<string, XNamespace> ns)
    {
        var temperature = root
            .Descendants(ns["iwxxm"] + "airTemperature")
            .FirstOrDefault()?
            .Value;

        if (decimal.TryParse(temperature, System.Globalization.CultureInfo.InvariantCulture, out var temp))
            return (int)Math.Round(temp);

        return null;
    }

    private int? ExtractDewpoint(XElement root, Dictionary<string, XNamespace> ns)
    {
        var dewpoint = root
            .Descendants(ns["iwxxm"] + "dewpointTemperature")
            .FirstOrDefault()?
            .Value;

        if (decimal.TryParse(dewpoint, System.Globalization.CultureInfo.InvariantCulture, out var dew))
            return (int)Math.Round(dew);

        return null;
    }

    private int? ExtractAltimeter(XElement root, Dictionary<string, XNamespace> ns)
    {
        var qnh = root
            .Descendants(ns["iwxxm"] + "qnh")
            .FirstOrDefault()?
            .Value;

        if (decimal.TryParse(qnh, System.Globalization.CultureInfo.InvariantCulture, out var altimeter))
            return (int)Math.Round(altimeter);

        return null;
    }

    private string? ExtractRemarks(XElement root, Dictionary<string, XNamespace> ns)
    {
        // Remarks field is not present in current IWXXM structure
        return null;
    }
}