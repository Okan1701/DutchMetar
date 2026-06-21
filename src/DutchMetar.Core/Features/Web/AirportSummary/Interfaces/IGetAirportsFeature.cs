namespace DutchMetar.Core.Features.Web.AirportSummary.Interfaces;

public interface IGetAirportSummariesFeature
{
    Task<ICollection<Models.SingleAirportSummary>> GetAirportSummariesAsync();
}