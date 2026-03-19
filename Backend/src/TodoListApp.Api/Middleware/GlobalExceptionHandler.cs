using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Api.Middleware;

/// <summary>
/// Provides a centralized global exception handling mechanism by implementing <see cref="IExceptionHandler"/>.
/// </summary>
/// <remarks>
/// This handler intercepts unhandled exceptions in the request pipeline, logs them based on severity,
/// and returns standardized error responses following the RFC 7807 (Problem Details for HTTP APIs) specification.
/// </remarks>
/// <param name="logger">The logger used for recording error details, warnings, and stack traces.</param>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle the specified exception and writes a standardized JSON response to the client.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <param name="exception">The exception that occurred during request processing.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="ValueTask"/> that returns <c>true</c> if the exception was handled successfully;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The handler maps specific exception types (e.g., <see cref="DomainException"/>, <see cref="PasswordChangeRequiredException"/>)
    /// to appropriate HTTP status codes and filters logging levels to avoid noise from business-rule violations.
    /// </remarks>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            PasswordChangeRequiredException => (StatusCodes.Status403Forbidden, "Password Change Required"),
            DomainException => (StatusCodes.Status400BadRequest, "Business Rule Violation"),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        if (statusCode >= 500)
        {
            logger.LogError(exception, "Unhandled exception");
        }
        else if (exception is KeyNotFoundException)
        {
            logger.LogWarning(exception, "Resource not found");
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception switch
            {
                PasswordChangeRequiredException => exception.Message,
                DomainException => exception.Message,
                UnauthorizedAccessException => exception.Message,
                _ => "An unexpected error occurred."
            },
            Instance = httpContext.TraceIdentifier,
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
