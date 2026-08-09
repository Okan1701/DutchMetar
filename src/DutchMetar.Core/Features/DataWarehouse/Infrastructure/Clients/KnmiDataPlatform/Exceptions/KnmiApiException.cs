using System.Net;

namespace DutchMetar.Core.Features.DataWarehouse.Infrastructure.Clients.KnmiDataPlatform.Exceptions;

public class KnmiApiException : Exception
{
    public HttpStatusCode StatusCode { get; set; }
    
    public KnmiApiException(HttpStatusCode statusCode, string? message) : base(message)
    {
    }

    public KnmiApiException(HttpStatusCode statusCode, string? message, Exception? innerException) : base(message, innerException)
    {
    }
}