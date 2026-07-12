using System.Net;

namespace DutchMetar.Core.Features.DataWarehouse.Shared.Infrastructure.Clients.KnmiDataPlatform.Exceptions;

public class KnmiRateLimitReachedException : KnmiApiException
{
    public KnmiRateLimitReachedException() : base(HttpStatusCode.TooManyRequests, "Max Request limit has been reached")
    {
    }

    public KnmiRateLimitReachedException(HttpStatusCode statusCode, string? message) : base(statusCode, message)
    {
    }
}