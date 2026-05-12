using GymManagement.Application.Exceptions;
using GymManagement.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

namespace GymManagement.Infrastructure.Middleware;

public sealed class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, title) = MapException(exception);

        if (statusCode >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception for {Path}", context.Request.Path);
        else
            logger.LogWarning(exception, "Request failed with {StatusCode} for {Path}", statusCode, context.Request.Path);

        context.Response.StatusCode = statusCode;

        var details = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = context.Request.Path
        };

        details.Extensions["code"] = code;
        details.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsJsonAsync(details);
    }

    private static (int StatusCode, string Code, string Title) MapException(Exception exception)
    {
        if (exception is NotFoundException)
            return (StatusCodes.Status404NotFound, "NotFound", "Resource was not found.");

        if (exception is DomainError domainError)
            return MapDomainError(domainError);

        return (StatusCodes.Status500InternalServerError, "ServerError", "An unexpected error occurred.");
    }

    private static (int StatusCode, string Code, string Title) MapDomainError(DomainError error)
    {
        if (IsUnauthorized(error.Code))
            return (StatusCodes.Status401Unauthorized, error.Code, "Authentication failed.");

        if (IsNotFound(error.Code))
            return (StatusCodes.Status404NotFound, error.Code, "Resource was not found.");

        if (IsConflict(error.Code))
            return (StatusCodes.Status409Conflict, error.Code, "Request conflicts with current state.");

        return (StatusCodes.Status400BadRequest, error.Code, "Request is invalid.");
    }

    private static bool IsUnauthorized(string code)
        => code.EndsWith(".InvalidCredentials", StringComparison.Ordinal);

    private static bool IsNotFound(string code)
        => code.EndsWith(".NotFound", StringComparison.Ordinal)
           || code.Contains("NotFound", StringComparison.Ordinal);

    private static bool IsConflict(string code)
        => code.EndsWith(".AlreadyExists", StringComparison.Ordinal)
           || code.EndsWith(".InUse", StringComparison.Ordinal)
           || code.Contains(".HasActive", StringComparison.Ordinal)
           || code.Contains(".HasFuture", StringComparison.Ordinal)
           || code.Contains(".Conflict", StringComparison.Ordinal)
           || code.EndsWith(".ActiveExists", StringComparison.Ordinal)
           || code.EndsWith(".AlreadyEnrolled", StringComparison.Ordinal)
           || code.EndsWith(".DuplicateEnrollment", StringComparison.Ordinal)
           || code.EndsWith(".Full", StringComparison.Ordinal);
}
