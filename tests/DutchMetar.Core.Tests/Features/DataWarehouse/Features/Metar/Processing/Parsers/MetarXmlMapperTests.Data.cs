namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Metar.Processing.Parsers;

public partial class MetarXmlParserTests
{
    private const string Eham051353Xml = """
        0000431201
        LANL80 EHAM 051353
        <?xml version="1.0" ?>
        <iwxxm:METAR automatedStation="false" gml:id="uuid.a8698a8a-955a-493b-ae82-59693d57e863" permissibleUsage="OPERATIONAL" reportStatus="NORMAL" translatedBulletinID="" translatedBulletinReceptionTime="2026-07-05T13:53:23Z" translationCentreDesignator="EHAM" translationCentreName="MetConsole" translationTime="2026-07-05T13:53:23Z" xmlns:aixm="http://www.aixm.aero/schema/5.1.1" xmlns:gml="http://www.opengis.net/gml/3.2" xmlns:iwxxm="http://icao.int/iwxxm/3.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://icao.int/iwxxm/3.0 http://schemas.wmo.int/iwxxm/3.0/iwxxm.xsd
        http://def.wmo.int/metce/2013 http://schemas.wmo.int/metce/1.2/metce.xsd">
        <!-- METAR EHAM 051355Z 29015KT 260V320 9999 FEW014 BKN016 BKN024 19/15 Q1022 TEMPO SCT016= -->
        <iwxxm:issueTime>
        <gml:TimeInstant gml:id="uuid.2c75fb6e-450a-483c-891b-95411ddb9933">
        <gml:timePosition>2026-07-05T13:55:00Z</gml:timePosition>
        </gml:TimeInstant>
        </iwxxm:issueTime>
        <iwxxm:aerodrome>
        <aixm:AirportHeliport gml:id="uuid.04c9a065-470f-400c-bb1a-ba50af36be88">
        <aixm:timeSlice>
        <aixm:AirportHeliportTimeSlice gml:id="uuid.c42dc080-0c85-4efb-a6af-3ca12f0897e4">
        <gml:validTime/>
        <aixm:interpretation>SNAPSHOT</aixm:interpretation>
        <aixm:designator>EHAM</aixm:designator>
        <aixm:name>AMSTERDAM AIRPORT SCHIPHOL</aixm:name>
        <aixm:locationIndicatorICAO>EHAM</aixm:locationIndicatorICAO>
        </aixm:AirportHeliportTimeSlice>
        </aixm:timeSlice>
        </aixm:AirportHeliport>
        </iwxxm:aerodrome>
        <iwxxm:observationTime>
        <gml:TimeInstant gml:id="uuid.a8a26102-63bd-4d24-a00c-cb650f37bd8c">
        <gml:timePosition>2026-07-05T13:55:00Z</gml:timePosition>
        </gml:TimeInstant>
        </iwxxm:observationTime>
        <iwxxm:observation>
        <iwxxm:MeteorologicalAerodromeObservation cloudAndVisibilityOK="false" gml:id="uuid.a05a2f14-4d36-4029-9c14-fe889a29884e">
        <iwxxm:airTemperature uom="Cel">19.0</iwxxm:airTemperature>
        <iwxxm:dewpointTemperature uom="Cel">15.0</iwxxm:dewpointTemperature>
        <iwxxm:qnh uom="hPa">1022.0</iwxxm:qnh>
        <iwxxm:surfaceWind>
        <iwxxm:AerodromeSurfaceWind variableWindDirection="false">
        <iwxxm:meanWindDirection uom="deg">290.0</iwxxm:meanWindDirection>
        <iwxxm:meanWindSpeed uom="[kn_i]">15.0</iwxxm:meanWindSpeed>
        <iwxxm:extremeClockwiseWindDirection uom="deg">260.0</iwxxm:extremeClockwiseWindDirection>
        <iwxxm:extremeCounterClockwiseWindDirection uom="deg">320.0</iwxxm:extremeCounterClockwiseWindDirection>
        </iwxxm:AerodromeSurfaceWind>
        </iwxxm:surfaceWind>
        <iwxxm:visibility>
        <iwxxm:AerodromeHorizontalVisibility>
        <iwxxm:prevailingVisibility uom="m">10000.0</iwxxm:prevailingVisibility>
        <iwxxm:prevailingVisibilityOperator>ABOVE</iwxxm:prevailingVisibilityOperator>
        </iwxxm:AerodromeHorizontalVisibility>
        </iwxxm:visibility>
        <iwxxm:cloud>
        <iwxxm:AerodromeCloud>
        <iwxxm:layer>
        <iwxxm:CloudLayer>
        <iwxxm:amount xlink:href="http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/FEW"/>
        <iwxxm:base uom="[ft_i]">1400.0</iwxxm:base>
        </iwxxm:CloudLayer>
        </iwxxm:layer>
        <iwxxm:layer>
        <iwxxm:CloudLayer>
        <iwxxm:amount xlink:href="http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/BKN"/>
        <iwxxm:base uom="[ft_i]">1600.0</iwxxm:base>
        </iwxxm:CloudLayer>
        </iwxxm:layer>
        <iwxxm:layer>
        <iwxxm:CloudLayer>
        <iwxxm:amount xlink:href="http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/BKN"/>
        <iwxxm:base uom="[ft_i]">2400.0</iwxxm:base>
        </iwxxm:CloudLayer>
        </iwxxm:layer>
        </iwxxm:AerodromeCloud>
        </iwxxm:cloud>
        </iwxxm:MeteorologicalAerodromeObservation>
        </iwxxm:observation>
        <iwxxm:trendForecast>
        <iwxxm:MeteorologicalAerodromeTrendForecast changeIndicator="TEMPORARY_FLUCTUATIONS" cloudAndVisibilityOK="false" gml:id="uuid.27b2de63-b336-43cc-8e0f-8f5eb9a880f1">
        <iwxxm:phenomenonTime nilReason="http://codes.wmo.int/common/nil/missing"/>
        <iwxxm:cloud>
        <iwxxm:AerodromeCloudForecast gml:id="uuid.47e27eb6-a6f1-4403-afb0-eb01d82cd0c5">
        <iwxxm:layer>
        <iwxxm:CloudLayer>
        <iwxxm:amount xlink:href="http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/SCT"/>
        <iwxxm:base uom="[ft_i]">1600.0</iwxxm:base>
        </iwxxm:CloudLayer>
        </iwxxm:layer>
        </iwxxm:AerodromeCloudForecast>
        </iwxxm:cloud>
        </iwxxm:MeteorologicalAerodromeTrendForecast>
        </iwxxm:trendForecast>
        </iwxxm:METAR>
        """;

    private const string Ehbk051352Xml = """
        0000371601
        LANL80 EHBK 051352
        <?xml version="1.0" ?>
        <iwxxm:METAR automatedStation="true" gml:id="uuid.ae3ef47e-b3d3-4284-b98b-af82e7418d38" permissibleUsage="OPERATIONAL" reportStatus="NORMAL" translatedBulletinID="" translatedBulletinReceptionTime="2026-07-05T13:52:01Z" translationCentreDesignator="EHBK" translationCentreName="MetConsole" translationTime="2026-07-05T13:52:01Z" xmlns:aixm="http://www.aixm.aero/schema/5.1.1" xmlns:gml="http://www.opengis.net/gml/3.2" xmlns:iwxxm="http://icao.int/iwxxm/3.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://icao.int/iwxxm/3.0 http://schemas.wmo.int/iwxxm/3.0/iwxxm.xsd
        http://def.wmo.int/metce/2013 http://schemas.wmo.int/metce/1.2/metce.xsd">
        <!-- METAR EHBK 051355Z AUTO 30007KT 240V360 9999 BKN027 BKN035 OVC041 21/15 Q1022 NOSIG= -->
        <iwxxm:issueTime>
        <gml:TimeInstant gml:id="uuid.05448039-6d24-4ddb-94ae-0798bb4c1017">
        <gml:timePosition>2026-07-05T13:55:00Z</gml:timePosition>
        </gml:TimeInstant>
        </iwxxm:issueTime>
        <iwxxm:aerodrome>
        <aixm:AirportHeliport gml:id="uuid.22cf0083-c35b-4a46-97b4-31b5c9603738">
        <aixm:timeSlice>
        <aixm:AirportHeliportTimeSlice gml:id="uuid.d0288243-f678-4cf2-a39c-c3caa7e83357">
        <gml:validTime/>
        <aixm:interpretation>SNAPSHOT</aixm:interpretation>
        <aixm:designator>EHBK</aixm:designator>
        <aixm:name>MAASTRICHT-AACHEN AIRPORT</aixm:name>
        <aixm:locationIndicatorICAO>EHBK</aixm:locationIndicatorICAO>
        </aixm:AirportHeliportTimeSlice>
        </aixm:timeSlice>
        </aixm:AirportHeliport>
        </iwxxm:aerodrome>
        <iwxxm:observationTime>
        <gml:TimeInstant gml:id="uuid.45918453-03b3-4a71-ab7d-a68e98b2c572">
        <gml:timePosition>2026-07-05T13:55:00Z</gml:timePosition>
        </gml:TimeInstant>
        </iwxxm:observationTime>
        <iwxxm:observation>
        <iwxxm:MeteorologicalAerodromeObservation cloudAndVisibilityOK="false" gml:id="uuid.2457faee-38f4-4f73-8633-c6e55b523288">
        <iwxxm:airTemperature uom="Cel">21.0</iwxxm:airTemperature>
        <iwxxm:dewpointTemperature uom="Cel">15.0</iwxxm:dewpointTemperature>
        <iwxxm:qnh uom="hPa">1022.0</iwxxm:qnh>
        <iwxxm:surfaceWind>
        <iwxxm:AerodromeSurfaceWind variableWindDirection="false">
        <iwxxm:meanWindDirection uom="deg">300.0</iwxxm:meanWindDirection>
        <iwxxm:meanWindSpeed uom="[kn_i]">7.0</iwxxm:meanWindSpeed>
        <iwxxm:extremeClockwiseWindDirection uom="deg">240.0</iwxxm:extremeClockwiseWindDirection>
        <iwxxm:extremeCounterClockwiseWindDirection uom="deg">360.0</iwxxm:extremeCounterClockwiseWindDirection>
        </iwxxm:AerodromeSurfaceWind>
        </iwxxm:surfaceWind>
        <iwxxm:visibility>
        <iwxxm:AerodromeHorizontalVisibility>
        <iwxxm:prevailingVisibility uom="m">10000.0</iwxxm:prevailingVisibility>
        <iwxxm:prevailingVisibilityOperator>ABOVE</iwxxm:prevailingVisibilityOperator>
        </iwxxm:AerodromeHorizontalVisibility>
        </iwxxm:visibility>
        <iwxxm:cloud>
        <iwxxm:AerodromeCloud>
        <iwxxm:layer>
        <iwxxm:CloudLayer>
        <iwxxm:amount xlink:href="http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/BKN"/>
        <iwxxm:base uom="[ft_i]">2700.0</iwxxm:base>
        </iwxxm:CloudLayer>
        </iwxxm:layer>
        <iwxxm:layer>
        <iwxxm:CloudLayer>
        <iwxxm:amount xlink:href="http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/BKN"/>
        <iwxxm:base uom="[ft_i]">3500.0</iwxxm:base>
        </iwxxm:CloudLayer>
        </iwxxm:layer>
        <iwxxm:layer>
        <iwxxm:CloudLayer>
        <iwxxm:amount xlink:href="http://codes.wmo.int/49-2/CloudAmountReportedAtAerodrome/OVC"/>
        <iwxxm:base uom="[ft_i]">4100.0</iwxxm:base>
        </iwxxm:CloudLayer>
        </iwxxm:layer>
        </iwxxm:AerodromeCloud>
        </iwxxm:cloud>
        </iwxxm:MeteorologicalAerodromeObservation>
        </iwxxm:observation>
        <iwxxm:trendForecast nilReason="http://codes.wmo.int/common/nil/noSignificantChange" xsi:nil="true"/>
        </iwxxm:METAR>
        """;
}
