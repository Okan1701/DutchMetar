using DutchMetar.Core.Features.AirportPerDayHistory.Models;

namespace DutchMetar.Core.Features.AirportPerDayHistory.Interfaces;

public interface IGetAirportDayHistoryFeature
{
    Task<AirportDayHistory> GetAirportDayHistoryAsync(GetAirportDayHistoryInput input,
        CancellationToken cancellationToken = default);
}