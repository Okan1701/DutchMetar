using DutchMetar.Core.Domain.Entities;
using DutchMetar.Core.Domain.Enums;
using DutchMetar.Core.Helpers.Enums;

namespace DutchMetar.Core.Helpers;

public static class MeteoConditionsHelper
{
    public static MetarMeteoCondition GetMeteoCondition(Metar metar)
    {
        if (metar.IsCavok || metar.NoCloudsDetected)
        {
            return MetarMeteoCondition.Vmc;
        }

        var clouds = metar.Ceilings ?? Enumerable.Empty<MetarCeiling>();
        var lowestSignificantCeiling = clouds
            .Where(x => x.Type is CeilingType.Broken or CeilingType.Overcast)
            .OrderBy(x => x.Height)
            .FirstOrDefault();
        var lowestCeilingHeight = lowestSignificantCeiling?.Height ?? int.MaxValue;
        
        if (metar.VisibilityMeters is < 5000 || lowestCeilingHeight <= 1000)
        {
            return MetarMeteoCondition.Imc;
        }

        return MetarMeteoCondition.Vmc;
    }
}