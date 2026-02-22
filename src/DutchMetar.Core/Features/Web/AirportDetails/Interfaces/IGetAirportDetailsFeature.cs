namespace DutchMetar.Core.Features.Web.AirportDetails.Interfaces;

public interface IGetAirportDetailsFeature
{
    Task<AirportDetails.Models.AirportDetails> GetAirportDetailsAsync(string airportCode, CancellationToken cancellationToken = default);
}