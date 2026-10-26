namespace DutchMetar.Core.Domain.Exceptions;

/// <summary>
/// Standard exception thrown when input contains invalid data.
/// </summary>
public class RequestValidationException : DutchMetarException
{
    public string[] ValidationErrors { get; } = Array.Empty<string>();

    public RequestValidationException(string? message) : base(message)
    {
        if (message != null)
        {
            ValidationErrors = [message];
        }
    }
    
    public RequestValidationException(string[] validationErrors)
    {
        ValidationErrors = validationErrors;
    }
}