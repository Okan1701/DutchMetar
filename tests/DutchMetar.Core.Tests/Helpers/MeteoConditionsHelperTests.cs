using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Helpers;
using DutchMetar.Core.Helpers.Enums;

namespace DutchMetar.Core.Tests.Helpers;

public class MeteoConditionsHelperTests
{
    [Fact]
    public void IsVmc_Cavok_ReturnsVmc()
    {
        var metar = new Metar
        {
            IsCavok = true,
            VisibilityMeters = 1000,
            RawMetar = "TEST"
        };

        var result = MeteoConditionsHelper.GetMeteoCondition(metar);

        Assert.Equal(MetarMeteoCondition.Vmc, result);
    }

    [Fact]
    public void IsVmc_NoCloudsDetected_ReturnsVmc()
    {
        var metar = new Metar
        {
            NoCloudsDetected = true,
            VisibilityMeters = 100,
            RawMetar = "TEST"
        };

        var result = MeteoConditionsHelper.GetMeteoCondition(metar);

        Assert.Equal(MetarMeteoCondition.Vmc, result);
    }

    [Fact]
    public void IsVmc_VisibilityBelow5000_ReturnsImc()
    {
        var metar = new Metar
        {
            VisibilityMeters = 4999,
            RawMetar = "TEST"
        };

        var result = MeteoConditionsHelper.GetMeteoCondition(metar);

        Assert.Equal(MetarMeteoCondition.Imc, result);
    }

    [Fact]
    public void IsVmc_VisibilityEquals5000_ReturnsVmc()
    {
        var metar = new Metar
        {
            VisibilityMeters = 5000,
            RawMetar = "TEST"
        };

        var result = MeteoConditionsHelper.GetMeteoCondition(metar);

        Assert.Equal(MetarMeteoCondition.Vmc, result);
    }

    [Fact]
    public void IsVmc_BrokenCloudAtOrBelow1000_ReturnsImc()
    {
        var metar = new Metar
        {
            VisibilityMeters = 10000,
            Ceilings = new List<MetarCeiling>
            {
                new() { Type = CeilingType.Broken, Height = 800 }
            },
            RawMetar = "TEST"
        };

        var result = MeteoConditionsHelper.GetMeteoCondition(metar);

        Assert.Equal(MetarMeteoCondition.Imc, result);
    }

    [Fact]
    public void IsVmc_ScatteredCloudOnlyBelow1000_ReturnsVmc()
    {
        var metar = new Metar
        {
            VisibilityMeters = 10000,
            Ceilings = new List<MetarCeiling>
            {
                new() { Type = CeilingType.Scattered, Height = 800 }
            },
            RawMetar = "TEST"
        };

        var result = MeteoConditionsHelper.GetMeteoCondition(metar);

        Assert.Equal(MetarMeteoCondition.Vmc, result);
    }

    [Fact]
    public void IsVmc_BrokenCloudAbove1000_ReturnsVmc()
    {
        var metar = new Metar
        {
            VisibilityMeters = 10000,
            Ceilings = new List<MetarCeiling>
            {
                new() { Type = CeilingType.Broken, Height = 1500 }
            },
            RawMetar = "TEST"
        };

        var result = MeteoConditionsHelper.GetMeteoCondition(metar);

        Assert.Equal(MetarMeteoCondition.Vmc, result);
    }

    [Fact]
    public void IsVmc_MultipleCeilings_LowestSignificantBelow1000_ReturnsImc()
    {
        var metar = new Metar
        {
            VisibilityMeters = 10000,
            Ceilings = new List<MetarCeiling>
            {
                new() { Type = CeilingType.Broken, Height = 2000 },
                new() { Type = CeilingType.Overcast, Height = 800 }
            },
            RawMetar = "TEST"
        };

        var result = MeteoConditionsHelper.GetMeteoCondition(metar);

        Assert.Equal(MetarMeteoCondition.Imc, result);
    }
}
