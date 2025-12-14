using DutchMetar.Web.Shared.Models;

namespace DutchMetar.Web.Client.Services.Interfaces;

public interface IAirportService
{
    Task<ICollection<AirportSummary>> GetAirportSummariesAsync();
}