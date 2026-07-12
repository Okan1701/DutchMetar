namespace DutchMetar.Core.Features.DataWarehouse.Shared.Exceptions;

/// <summary>
/// Thrown when METAR XML parsing fails.
/// </summary>
public class MetarXmlParsingException : Exception
{
    public MetarXmlParsingException(string message)
        : base(message)
    {
    }

    public MetarXmlParsingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
