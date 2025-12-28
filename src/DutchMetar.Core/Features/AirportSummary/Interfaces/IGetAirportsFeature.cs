namespace DutchMetar.Core.Features.AirportSummary.Interfaces;

public interface IGetAirportSummariesFeature
{
    Task<ICollection<Models.SingleAirportSummary>> GetAirportSummariesAsync();
}