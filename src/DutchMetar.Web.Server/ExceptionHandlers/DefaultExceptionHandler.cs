using System.Net;
using DutchMetar.Web.Server.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace DutchMetar.Web.Server.ExceptionHandlers;

public class DefaultExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(new ApiErrorModel
        {
            Status = HttpStatusCode.InternalServerError,
            RequestId = httpContext.TraceIdentifier,
            Title = "An unexpected error has occurred.",
        }, cancellationToken);

        return true;
    }
}