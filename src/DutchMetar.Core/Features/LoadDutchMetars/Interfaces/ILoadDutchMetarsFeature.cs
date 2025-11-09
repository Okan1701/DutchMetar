namespace DutchMetar.Core.Features.LoadDutchMetars.Interfaces;

public interface ILoadDutchMetarsFeature
{
    public Task LoadAsync(CancellationToken cancellationToken);
}