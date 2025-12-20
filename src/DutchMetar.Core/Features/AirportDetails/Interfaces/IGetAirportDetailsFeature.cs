namespace DutchMetar.Core.Features.AIrportDetails.Interfaces;

public interface IGetAirportDetailsFeature
{
    Task<AirportDetails.Models.AirportDetails> GetAirportDetailsAsync(string airportCode, CancellationToken cancellationToken = default);
}