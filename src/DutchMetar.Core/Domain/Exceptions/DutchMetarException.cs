namespace DutchMetar.Core.Domain.Exceptions;

/// <summary>
/// Base exception used for all DutchMetar exceptions
/// </summary>
public abstract class DutchMetarException : Exception
{
    protected DutchMetarException()
    {
    }

    protected DutchMetarException(string? message) : base(message)
    {
    }

    protected DutchMetarException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}