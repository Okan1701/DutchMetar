using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Features.DataWarehouse.Shared;
using DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;
using DutchMetar.Core.Features.DataWarehouse.Shared.Interfaces;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Shared;

public partial class MetarXmlMapperTests
{
    private readonly IMetarXmlMapper _mapper = new MetarXmlMapper();

    [Fact]
    public void Map_WithEham051353_MapsAllFieldsCorrectly()
    {
        var result = _mapper.Map(Eham051353Xml);

        Assert.NotNull(result.Ceilings);
        Assert.Equal("METAR EHAM 051355Z 29015KT 260V320 9999 FEW014 BKN016 BKN024 19/15 Q1022 TEMPO SCT016=",
            result.RawMetar);
        Assert.False(result.IsAuto);
        Assert.False(result.IsCavok);
        Assert.False(result.IsCorrected);
        Assert.Equal(290, result.WindDirection);
        Assert.Equal(15, result.WindSpeedKnots);
        Assert.Null(result.WindSpeedGustsKnots);
        Assert.Equal(10000, result.VisibilityMeters);
        Assert.Equal(19, result.TemperatureCelsius);
        Assert.Equal(15, result.DewpointCelsius);
        Assert.Equal(1022, result.AltimeterValue);
        Assert.False(result.NoCloudsDetected);
        Assert.Equal(new DateTimeOffset(2026, 7, 5, 13, 55, 0, TimeSpan.Zero), result.IssuedAt);
        Assert.Equal(TrendType.Tempo, result.TrendType);
        Assert.NotNull(result?.Ceilings);
        Assert.Equal(3, result?.Ceilings.Count);
        Assert.Contains(result!.Ceilings!, x => x is { Type: CeilingType.Few, Height: 1400 });
        Assert.Contains(result!.Ceilings!, x => x is { Type: CeilingType.Broken, Height: 1600 });
        Assert.Contains(result!.Ceilings!, x => x is { Type: CeilingType.Broken, Height: 2400 });
    }

    [Fact]
    public void Map_WithEhbk051352_MapsAllFieldsCorrectly()
    {
        var result = _mapper.Map(Ehbk051352Xml);

        Assert.NotNull(result);
        Assert.Equal("METAR EHBK 051355Z AUTO 30007KT 240V360 9999 BKN027 BKN035 OVC041 21/15 Q1022 NOSIG=",
            result.RawMetar);
        Assert.True(result.IsAuto);
        Assert.False(result.IsCavok);
        Assert.False(result.IsCorrected);
        Assert.Equal(300, result.WindDirection);
        Assert.Equal(7, result.WindSpeedKnots);
        Assert.Null(result.WindSpeedGustsKnots);
        Assert.Equal(10000, result.VisibilityMeters);
        Assert.Equal(21, result.TemperatureCelsius);
        Assert.Equal(15, result.DewpointCelsius);
        Assert.Equal(1022, result.AltimeterValue);
        Assert.False(result.NoCloudsDetected);
        Assert.Equal(TrendType.Nosig, result.TrendType);
        Assert.Equal(new DateTimeOffset(2026, 7, 5, 13, 55, 0, TimeSpan.Zero), result.IssuedAt);
    }

    [Fact]
    public void Map_WithNullInput_ThrowsMetarMappingException()
    {
        Assert.Throws<MetarMappingException>(() => _mapper.Map(null!));
    }

    [Fact]
    public void Map_WithEmptyInput_ThrowsMetarMappingException()
    {
        Assert.Throws<MetarMappingException>(() => _mapper.Map(string.Empty));
    }

    [Fact]
    public void Map_WithInvalidXml_ThrowsMetarMappingException()
    {
        Assert.Throws<MetarMappingException>(() => _mapper.Map("<invalid>xml</broken>"));
    }
}