using FluentValidation;
using JobTracker.AI.Application.Exceptions;
using JobTracker.AI.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.AI.Api.Exceptions;

/// <summary>
/// Maps inner-layer exceptions to RFC 7807 ProblemDetails. Same shape as the other services
/// (duplicated to keep services autonomous), with an AI-specific mapping: a provider failure
/// (<see cref="AiUnavailableException"/>) becomes 503, not a generic 500.
/// </summary>
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
            DomainException => (StatusCodes.Status422UnprocessableEntity, "The request could not be processed."),
            AiUnavailableException => (StatusCodes.Status503ServiceUnavailable, "The AI service is temporarily unavailable."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        // Log server-side faults (500) and upstream failures (503); expected 400/422 stay quiet.
        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Request to {Path} failed", httpContext.Request.Path);
        }

        var detail = exception switch
        {
            ValidationException => null,
            DomainException or AiUnavailableException => exception.Message,
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
