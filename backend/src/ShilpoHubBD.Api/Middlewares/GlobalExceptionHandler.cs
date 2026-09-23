using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Exceptions;

namespace ShilpoHubBD.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, errors) = exception switch
        {
            FluentValidation.ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed.",
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())),
            ConflictException conflictException => (
                StatusCodes.Status409Conflict, conflictException.Message, null),
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound, notFoundException.Message, null),
            UnauthorizedAccessException unauthorizedException => (
                StatusCodes.Status401Unauthorized, unauthorizedException.Message, null),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            if (exception is DbUpdateConcurrencyException concurrencyException)
            {
                foreach (var entry in concurrencyException.Entries)
                {
                    var keys = string.Join(", ", entry.Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .Select(p => $"{p.Metadata.Name}={p.CurrentValue}"));
                    _logger.LogError("DIAG concurrency entry: {Type} State={State} Keys={Keys}",
                        entry.Entity.GetType().Name, entry.State, keys);
                }
            }
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Instance = httpContext.Request.Path,
        };

        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
