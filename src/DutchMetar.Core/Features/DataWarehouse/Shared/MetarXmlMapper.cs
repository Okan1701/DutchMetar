using System.Xml.Linq;
using System.Text.RegularExpressions;
using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;

namespace DutchMetar.Core.Features.DataWarehouse.Shared;

public class MetarXmlMapper : IMetarXmlMapper
{
    public Metar Map(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
            throw new MetarMappingException("XML content cannot be null or empty.");

        var cleanedXml = CleanXmlContent(xmlContent);
        XDocument document;
        try
        {
            document = XDocument.Parse(cleanedXml);
        }
        catch (Exception ex)
        {
            throw new MetarMappingException("Failed to parse XML.", ex);
        }

        var root = document.Root;

        if (root == null)
            throw new MetarMappingException("XML document has no root element.");

        var ns = new Dictionary<string, XNamespace>
        {
            { "iwxxm", XNamespace.Get("http://icao.int/iwxxm/3.0") },
            { "gml", XNamespace.Get("http://www.opengis.net/gml/3.2") },
            { "aixm", XNamespace.Get("http://www.aixm.aero/schema/5.1.1") },
            { "xlink", XNamespace.Get("http://www.w3.org/1999/xlink") },
            { "xsi", XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance") },
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
            NoCloudsDetected = ExtractNoCloudsDetected(root, ns),
            TrendType = ExtractTrendType(root, ns),
            Ceilings = ExtractCeilings(root, ns),
            Airport = ExtractAirport(root, ns)
        };

        return metar;
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
        // TODO: Figure out where in the XML this is normally present.
        return null;
    }

    private TrendType ExtractTrendType(XElement root, Dictionary<string, XNamespace> ns)
    {
        var trendForecastElement = root.Element(ns["iwxxm"] + "trendForecast");
        var isNil = bool.TryParse(trendForecastElement?.Attribute(ns["xsi"] + "nil")?.Value, out var isNilParsed) && isNilParsed;
        var nilReason = trendForecastElement?.Attribute("nilReason")?.Value ?? string.Empty;

        if (isNil && nilReason == "http://codes.wmo.int/common/nil/noSignificantChange") return TrendType.Nosig;
        
        var forecastElement =  trendForecastElement?.Element(ns["iwxxm"] + "MeteorologicalAerodromeTrendForecast");
        if (forecastElement == null) return TrendType.None;
        
        var indicatorValue = forecastElement.Attribute("changeIndicator")?.Value ?? string.Empty;
        return indicatorValue == "TEMPORARY_FLUCTUATIONS" ? TrendType.Tempo : TrendType.None;
    }

    private MetarCeiling[] ExtractCeilings(XElement root, Dictionary<string, XNamespace> ns)
    {
        // Only consider cloud layers from the main MeteorologicalAerodromeObservation (not trendForecast)
        var observation = root.Element(ns["iwxxm"] + "observation");
        var meteorologicalObs = observation?.Element(ns["iwxxm"] + "MeteorologicalAerodromeObservation");

        if (meteorologicalObs == null)
            return Array.Empty<MetarCeiling>();

        var cloudLayers = meteorologicalObs
            .Descendants(ns["iwxxm"] + "CloudLayer")
            .ToArray();

        return cloudLayers.Select(layer =>
        {
            var amount = layer
                .Descendants(ns["iwxxm"] + "amount")
                .Attributes(ns["xlink"] + "href")
                .FirstOrDefault()?.Value ?? string.Empty;
            
            var layerBase = layer
                .Descendants(ns["iwxxm"] + "base")
                .FirstOrDefault()?.Value ?? string.Empty;
            
            var height = -1;
            if (decimal.TryParse(layerBase, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var baseVal))
            {
                height = (int)Math.Round(baseVal);
            }

            return new MetarCeiling
            {
                Type = amount switch
                {
                    "http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/FEW" => CeilingType.Few,
                    "http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/SCT" => CeilingType.Scattered,
                    "http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/BKN" => CeilingType.Broken,
                    "http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/OVC" => CeilingType.Overcast,
                    _ => CeilingType.Other
                },
                Height = height,
            };
        }).ToArray();
        
    }

    private Airport? ExtractAirport(XElement root, Dictionary<string, XNamespace> ns)
    {
        var airportHeliportTimeSlice = root?
            .Descendants(ns["aixm"] + "AirportHeliportTimeSlice")
            .FirstOrDefault();

        if (airportHeliportTimeSlice == null) return null;

        return new Airport
        {
            Icao = airportHeliportTimeSlice.Element(ns["aixm"] + "locationIndicatorICAO")?.Value ?? string.Empty,
            Name = airportHeliportTimeSlice.Element(ns["aixm"] + "name")?.Value ?? string.Empty
        };
    }
}