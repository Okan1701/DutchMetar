namespace DutchMetar.Core.Features.DataWarehouse.Features.Taf.Parsers;

public class TafParsingException : Exception
{
    public TafParsingException(string message)
        : base(message)
    {
    }

    public TafParsingException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}