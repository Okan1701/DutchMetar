using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Features.SyncKnmiMetar.Services;
using MetarParserCore;

namespace DutchMetar.Core.Tests.Features.SyncKnmiMetarFileList.Services;

public class MetarMapperTests
{
    private readonly MetarMapper _metarMapper = new();

    [Fact]
    public void MapDecodedMetarToEntity_InvalidMetarObject_ThrowsException()
    {
        var invalidDecodedMetar = new MetarParserCore.Objects.Metar();
        
        Assert.Throws<NullReferenceException>(() =>  _metarMapper.MapDecodedMetarToEntity(invalidDecodedMetar, "", DateTimeOffset.Now));
    }

    [Fact]
    public void MapDecodedMetarToEntity_NilMetar_ReturnsMappedEntity()
    {
        const string rawMetar =
            "METAR EHJR 091225Z NIL=";
        var metarObject = new MetarParser().Parse(rawMetar);

        var mappedEntity = _metarMapper.MapDecodedMetarToEntity(metarObject, rawMetar, DateTimeOffset.Now);
        
        Assert.NotNull(mappedEntity);
        Assert.Null(mappedEntity.AltimeterValue);
        Assert.Null(mappedEntity.DewpointCelsius);
        Assert.Null(mappedEntity.TemperatureCelsius);
        Assert.Null(mappedEntity.WindDirection);
        Assert.Null(mappedEntity.WindSpeedKnots);
        Assert.Null(mappedEntity.WindSpeedGustsKnots);
    }
    
    [Fact]
    public void MapDecodedMetarToEntity_CorrectedMetarObject_ReturnsMappedEntity()
    {
        const string rawMetar =
            "METAR EHKD 271355Z AUTO COR 09007KT 060V120 9999 15/11 Q1021 BLU NOSIG=";
        var metarObject = new MetarParser().Parse(rawMetar);

        var mappedEntity = _metarMapper.MapDecodedMetarToEntity(metarObject, rawMetar, DateTimeOffset.Now);
        
        Assert.NotNull(mappedEntity);
        Assert.True(mappedEntity.IsCorrected);
    }
    
    [Fact]
    public void MapDecodedMetarToEntity_ValidMetarObject_ReturnsMappedEntity()
    {
        const string rawMetar =
            "METAR EHKD 271355Z AUTO 09007KT 060V120 9999 15/11 Q1021 BLU NOSIG=";
        var metarObject = new MetarParser().Parse(rawMetar);

        var mappedEntity = _metarMapper.MapDecodedMetarToEntity(metarObject, rawMetar, DateTimeOffset.Now);

        var expectedIssuedAtDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 27, 13, 55, 0);
        Assert.NotNull(mappedEntity);
        Assert.NotNull(mappedEntity.RawMetar);
        Assert.Equal(1021, mappedEntity.AltimeterValue);
        Assert.Equal(15, mappedEntity.TemperatureCelsius);
        Assert.Equal(11, mappedEntity.DewpointCelsius);
        Assert.Equal(9999, mappedEntity.VisibilityMeters);
        Assert.Equal(90, mappedEntity.WindDirection);
        Assert.Equal(7, mappedEntity.WindSpeedKnots);
        Assert.Null(mappedEntity.WindSpeedGustsKnots);
        Assert.Null(mappedEntity.Remarks);
        Assert.Equal(expectedIssuedAtDateTime.Date, mappedEntity.IssuedAt.Date);
    }
    
    [Fact]
    public void MapDecodedMetarToEntity_ValidMetarWithGust_ReturnsMappedEntityWithGustValue()
    {
        const string rawMetar =
            "METAR EHKD 271355Z AUTO 09015G30KT 060V120 9999 15/11 Q1021 BLU NOSIG=";
        var metarObject = new MetarParser().Parse(rawMetar);

        var mappedEntity = _metarMapper.MapDecodedMetarToEntity(metarObject, rawMetar, DateTimeOffset.Now);

        var expectedIssuedAtDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 27, 13, 55, 0);
        Assert.NotNull(mappedEntity);
        Assert.NotNull(mappedEntity.RawMetar);
        Assert.Equal(90, mappedEntity.WindDirection);
        Assert.Equal(15, mappedEntity.WindSpeedKnots);
        Assert.Equal(30, mappedEntity.WindSpeedGustsKnots);
        Assert.Null(mappedEntity.Remarks);
        Assert.Equal(expectedIssuedAtDateTime.Date, mappedEntity.IssuedAt.Date);
    }
    
    [Fact]
    public void MapDecodedMetarToEntity_WithCloudCeilings_ReturnsMappedCeilingEntities()
    {
        const string rawMetar =
            "METAR EHKD 271355Z AUTO 09007KT 060V120 9999 FEW010 SCT015 SCT020 BKN025 OVC090 15/11 Q1021 BLU NOSIG=";
        var metarObject = new MetarParser().Parse(rawMetar);

        var mappedEntity = _metarMapper.MapDecodedMetarToEntity(metarObject, rawMetar, DateTimeOffset.Now);
        
        Assert.NotNull(mappedEntity);
        Assert.NotNull(mappedEntity.Ceilings);
        Assert.Equal(5, mappedEntity.Ceilings.Count);
        Assert.Contains(mappedEntity.Ceilings, x => x is { Type: CeilingType.Few, Height: 10 });
        Assert.Contains(mappedEntity.Ceilings, x => x is { Type: CeilingType.Scattered, Height: 15 });
        Assert.Contains(mappedEntity.Ceilings, x => x is { Type: CeilingType.Scattered, Height: 20 });
        Assert.Contains(mappedEntity.Ceilings, x => x is { Type: CeilingType.Broken, Height: 25 });
        Assert.Contains(mappedEntity.Ceilings, x => x is { Type: CeilingType.Overcast, Height: 90 });
    }
    
    [Fact]
    public void MapDecodedMetarToEntity_ValidMetarCavok_ReturnsCavokEntity()
    {
        const string rawMetar =
            "METAR EHAM 051050Z 28010KT CAVOK 18/M02 Q1012=";
        var metarObject = new MetarParser().Parse(rawMetar);

        var mappedEntity = _metarMapper.MapDecodedMetarToEntity(metarObject, rawMetar, DateTimeOffset.Now);
        
        Assert.NotNull(mappedEntity);
        Assert.True(mappedEntity.IsCavok);
    }
    
    [Fact]
    public void MapDecodedMetarToEntity_ValidMetarNcd_ReturnsNcdTrue()
    {
        const string rawMetar =
            "METAR EHAM 051050Z 28010KT NCD 18/M02 Q1012=";
        var metarObject = new MetarParser().Parse(rawMetar);

        var mappedEntity = _metarMapper.MapDecodedMetarToEntity(metarObject, rawMetar, DateTimeOffset.Now);
        
        Assert.NotNull(mappedEntity);
        Assert.True(mappedEntity.NoCloudsDetected);
    }
}