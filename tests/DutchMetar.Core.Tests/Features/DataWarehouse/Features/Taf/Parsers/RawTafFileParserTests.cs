using DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;

namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Taf.Parsers;

public partial class RawTafFileParserTests
{
    private readonly IRawTafFileParser _parser = new RawTafFileParser();

    [Fact]
    public void ParseRawTafToEntity_WithEhrdPayload_MapsAllFieldsCorrectly()
    {
        var result = _parser.ParseRawTafToEntity(EhrdTafPayload);

        Assert.NotNull(result.Airport);
        Assert.Equal("EHRD", result.Airport!.Icao);
        Assert.Equal(0, result.AirportId);
        Assert.Equal(new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 15, 4, 58, 0, TimeSpan.Zero), result.IssuedAt);
        Assert.Equal(
            "TAF EHRD 150458Z 1506/1612 03006KT CAVOK PROB30 TEMPO 1506/1511 -SHRA FEW080CB BECMG 1512/1514 02012KT BECMG 1519/1521 02006KT=",
            result.RawTaf);
    }

    [Fact]
    public void ParseRawTafToEntity_WithTncbPayload_MapsAllFieldsCorrectly()
    {
        var result = _parser.ParseRawTafToEntity(TncbTafPayload);

        Assert.NotNull(result.Airport);
        Assert.Equal("TNCB", result.Airport!.Icao);
        Assert.Equal(0, result.AirportId);
        Assert.Equal(new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 15, 4, 58, 0, TimeSpan.Zero), result.IssuedAt);
        Assert.Equal(
            "TAF TNCB 150458Z 1506/1612 08012KT 9999 FEW018 PROB30 TEMPO 1512/1522 6000 SHRA SCT016CB PROB40 TEMPO 1606/1612 4000 SHRA SCT016CB=",
            result.RawTaf);
    }

    [Fact]
    public void ParseRawTafToEntity_WithEhggPayload_MapsAllFieldsCorrectly()
    {
        var result = _parser.ParseRawTafToEntity(EhggTafPayload);

        Assert.NotNull(result.Airport);
        Assert.Equal("EHGG", result.Airport!.Icao);
        Assert.Equal(0, result.AirportId);
        Assert.Equal(new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 15, 7, 39, 0, TimeSpan.Zero), result.IssuedAt);
        Assert.Equal(
            "TAF AMD EHGG 150739Z 1507/1612 33004KT 9999 BKN007 TEMPO 1507/1512 7000 SHRA FEW090CB BECMG 1509/1512 02010KT CAVOK BECMG 1521/1524 34003KT=",
            result.RawTaf);
    }

    [Fact]
    public void ParseRawTafToEntity_WithUnparseableIssueTime_LeavesIssuedAtNull()
    {
        var result = _parser.ParseRawTafToEntity(InvalidIssuedAtTafPayload);

        Assert.NotNull(result.Airport);
        Assert.Equal("EHRD", result.Airport!.Icao);
        Assert.Null(result.IssuedAt);
        Assert.Equal("TAF EHRD 999999Z 1506/1612 03006KT CAVOK", result.RawTaf);
    }

    [Fact]
    public void ParseRawTafToEntity_WithNullInput_ThrowsTafParsingException()
    {
        Assert.Throws<TafParsingException>(() => _parser.ParseRawTafToEntity(null!));
    }

    [Fact]
    public void ParseRawTafToEntity_WithEmptyInput_ThrowsTafParsingException()
    {
        Assert.Throws<TafParsingException>(() => _parser.ParseRawTafToEntity(string.Empty));
    }

    [Fact]
    public void ParseRawTafToEntity_WithMissingTafKeyword_ThrowsTafParsingException()
    {
        Assert.Throws<TafParsingException>(() => _parser.ParseRawTafToEntity("ZCZC\nFT150500 EHRD"));
    }
}
