namespace DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure;

public class KnmiMetarApiOptions
{
    /// <summary>
    /// Authorization token used to authenticate with the KNMI API
    /// </summary>
    public required string AuthorizationToken { get; set; }
}