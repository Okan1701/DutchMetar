using System.Net;

namespace DutchMetar.Core.Features.SyncKnmiMetar.Infrastructure.Exceptions;

public class KnmiRateLimitReachedException : KnmiApiException
{
    public KnmiRateLimitReachedException() : base(HttpStatusCode.TooManyRequests, "Max Request limit has been reached")
    {
    }

    public KnmiRateLimitReachedException(HttpStatusCode statusCode, string? message) : base(statusCode, message)
    {
    }
}