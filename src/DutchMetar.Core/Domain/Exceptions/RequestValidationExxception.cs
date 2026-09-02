namespace DutchMetar.Core.Domain.Exceptions;

/// <summary>
/// Standard exception thrown when input contains invalid data.
/// </summary>
public class RequestValidationExxception : DutchMetarException
{
    private readonly string[] _validationErrors = Array.Empty<string>();

    public RequestValidationExxception(string? message) : base(message)
    {
        if (message != null)
        {
            _validationErrors = [message];
        }
    }
    
    public RequestValidationExxception(string[] validationErrors)
    {
        _validationErrors = validationErrors;
    }
}