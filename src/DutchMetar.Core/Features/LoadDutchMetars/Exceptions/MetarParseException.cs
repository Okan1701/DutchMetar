using DutchMetar.Core.Domain.Exceptions;

namespace DutchMetar.Core.Features.LoadDutchMetars.Exceptions;

public class MetarParseException : DutchMetarException
{
    public MetarParseException(string? message) : base(message)
    {
    }
}