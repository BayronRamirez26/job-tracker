using FluentValidation;
using JobTracker.Applications.Application.JobApplications;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.Applications.Application;

/// <summary>
/// Registers the Application layer with the DI container. Each layer exposes its own
/// <c>Add*</c> extension so the API's composition root can wire the whole graph up with one
/// call per layer, keeping <c>Program.cs</c> readable.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use FluentValidation's built-in (English) messages regardless of the host's OS culture,
        // so error text stays consistent. Our explicit .WithMessage(...) rules are unaffected.
        ValidatorOptions.Global.LanguageManager.Enabled = false;

        // Scan this assembly and register every AbstractValidator<T> as IValidator<T> (scoped).
        services.AddValidatorsFromAssemblyContaining<JobApplicationService>();

        services.AddScoped<IJobApplicationService, JobApplicationService>();

        return services;
    }
}
