using DutchMetar.Web.Shared.Models;

namespace DutchMetar.Web.Shared.Services.Interfaces;

public interface IAirportService
{
    Task<ICollection<AirportSummary>> GetAirportSummariesAsync();

    Task<AirportDetails> GetAirportDetailsAsync(string airportIcaoCode);

    Task<AirportDayHistory> GetAirportHistoryAsync(string airportIcaoCode, DateOnly date);
}