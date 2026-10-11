namespace DutchMetar.Core.Features.Web.TafHistory;

public interface IGetTafHistoryFeature
{
    Task<GetTafHistoryResult> GetHistoryAsync(GetTafHistoryRequest request, CancellationToken cancellationToken = default);
}
