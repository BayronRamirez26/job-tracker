using FluentValidation;
using JobTracker.Applications.Application.Exceptions;
using JobTracker.Applications.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Applications.Api.Exceptions;

/// <summary>
/// Translates exceptions thrown by the inner layers into RFC 7807 <see cref="ProblemDetails"/>
/// responses with the correct HTTP status code. This is the single place where "which exception
/// means which status" is decided, so controllers and services never touch HTTP concerns.
/// </summary>
/// <remarks>
/// Mapping:
/// <list type="bullet">
/// <item>FluentValidation <c>ValidationException</c> → 400 (with per-field <c>errors</c>)</item>
/// <item><c>NotFoundException</c> → 404</item>
/// <item><c>DomainException</c> → 422 (well-formed request, but a business rule was violated)</item>
/// <item>anything else → 500 (logged; no internal detail leaked to the client)</item>
/// </list>
/// </remarks>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "One or more validation errors occurred."),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "The request could not be processed."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            // Log the full detail server-side; the client only sees a generic message.
            _logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);
        }

        // Validation specifics live in the `errors` extension, not in a free-text detail.
        // NotFound/Domain messages are safe and useful; 500s must not leak internals.
        var detail = exception switch
        {
            ValidationException => null,
            NotFoundException or DomainException => exception.Message,
            _ => null
        };

        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
