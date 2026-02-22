using DutchMetar.Core.Features.Web.AirportPerDayHistory.Models;

namespace DutchMetar.Core.Features.Web.AirportPerDayHistory.Interfaces;

public interface IGetAirportDayHistoryFeature
{
    Task<AirportDayHistory> GetAirportDayHistoryAsync(GetAirportDayHistoryInput input,
        CancellationToken cancellationToken = default);
}