namespace DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;

/// <summary>
/// Thrown when METAR XML mapping fails.
/// </summary>
public class MetarMappingException : Exception
{
    public MetarMappingException(string message)
        : base(message)
    {
    }

    public MetarMappingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
