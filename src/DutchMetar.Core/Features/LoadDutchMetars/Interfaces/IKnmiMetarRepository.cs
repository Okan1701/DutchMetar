namespace DutchMetar.Core.Features.LoadDutchMetars.Interfaces;

public interface IKnmiMetarRepository
{
    public Task<ICollection<string>> GetKnmiRawMetarsAsync();
}