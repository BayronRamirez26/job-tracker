using FluentValidation;
using JobTracker.Users.Application.Exceptions;
using JobTracker.Users.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Users.Api.Exceptions;

/// <summary>
/// Maps exceptions from the inner layers to RFC 7807 ProblemDetails responses. Same shape as the
/// Applications service's handler (duplicated to keep services autonomous), extended with the
/// auth-specific mappings: InvalidCredentials -> 401 and Conflict -> 409.
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
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Authentication failed."),
            ConflictException => (StatusCodes.Status409Conflict, "The request conflicts with existing state."),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found."),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "The request could not be processed."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);
        }

        var detail = exception switch
        {
            ValidationException => null,
            InvalidCredentialsException or ConflictException or NotFoundException or DomainException => exception.Message,
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
