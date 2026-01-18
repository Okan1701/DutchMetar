using DutchMetar.Core.Domain.Exceptions;

namespace DutchMetar.Core.Features.SyncKnmiMetar.Exceptions;

public class MetarParseException : DutchMetarException
{
    public MetarParseException(string? message) : base(message)
    {
    }

    public MetarParseException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}