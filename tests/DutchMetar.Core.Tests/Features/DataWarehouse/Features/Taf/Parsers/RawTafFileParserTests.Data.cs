namespace DutchMetar.Core.Tests.Features.DataWarehouse.Features.Taf.Parsers;

public partial class RawTafFileParserTests
{
    private const string EhrdTafPayload = """
        ZCZC
        FT150500 EHRD
        TAF EHRD 150458Z 1506/1612 03006KT CAVOK
            PROB30 TEMPO 1506/1511 -SHRA FEW080CB
            BECMG 1512/1514 02012KT
            BECMG 1519/1521 02006KT=
        """;

    private const string TncbTafPayload = """
        ZCZC
        FT150527 TNCB
        TAF TNCB 150458Z 1506/1612 08012KT 9999 FEW018
            PROB30 TEMPO 1512/1522 6000 SHRA SCT016CB
            PROB40 TEMPO 1606/1612 4000 SHRA SCT016CB=
        """;

    private const string EhggTafPayload = """
        ZCZC
        FT150500 EHGG AAA
        TAF AMD EHGG 150739Z 1507/1612 33004KT 9999 BKN007
            TEMPO 1507/1512 7000 SHRA FEW090CB
            BECMG 1509/1512 02010KT CAVOK
            BECMG 1521/1524 34003KT=
        """;

    private const string InvalidIssuedAtTafPayload = """
        ZCZC
        FT150500 EHRD
        TAF EHRD 999999Z 1506/1612 03006KT CAVOK
        """;
}
