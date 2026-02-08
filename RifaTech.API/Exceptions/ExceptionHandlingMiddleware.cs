using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace RifaTech.API.Exceptions
{
    /// <summary>
    /// Global exception handling middleware using ProblemDetails (RFC 7807).
    /// Registered via UseExceptionHandler in Program.cs.
    /// This class is kept for backward compatibility.
    /// </summary>
    public static class ExceptionHandlingMiddleware
    {
        public static void UseCustomExceptionHandling(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionFeature?.Error;

                    logger.LogError(exception, "Unhandled exception at {Path}", context.Request.Path);

                    var (statusCode, title) = exception switch
                    {
                        KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Resource not found"),
                        ArgumentException => ((int)HttpStatusCode.BadRequest, "Invalid request"),
                        UnauthorizedAccessException => ((int)HttpStatusCode.Forbidden, "Forbidden"),
                        InvalidOperationException => ((int)HttpStatusCode.Conflict, "Operation not allowed"),
                        _ => ((int)HttpStatusCode.InternalServerError, "Internal server error")
                    };

                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/problem+json";

                    await context.Response.WriteAsJsonAsync(new
                    {
                        type = $"https://httpstatuses.com/{statusCode}",
                        title,
                        status = statusCode,
                        detail = exception?.Message ?? "An error occurred processing your request.",
                        traceId = context.TraceIdentifier
                    });
                });
            });
        }
    }
}