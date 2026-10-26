using System.Net;
using DutchMetar.Core.Domain.Exceptions;
using DutchMetar.Web.Server.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace DutchMetar.Web.Server.ExceptionHandlers;

public class DomainExceptionHandlers : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is EntityNotFoundException notFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(new ApiErrorModel
            {
                Status = HttpStatusCode.NotFound,
                RequestId = httpContext.TraceIdentifier,
                Title = notFoundException.Message,
            }, cancellationToken);
            return true;
        }
        
        if (exception is RequestValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(new ApiErrorModel
            {
                Status = HttpStatusCode.BadRequest,
                RequestId = httpContext.TraceIdentifier,
                Title = "One or more validation errors occurred.",
                Errors = validationException.ValidationErrors
            }, cancellationToken);
            return true;
        }
        
        return false;
    }
}