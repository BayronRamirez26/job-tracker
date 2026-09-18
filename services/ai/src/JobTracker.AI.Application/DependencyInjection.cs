using FluentValidation;
using JobTracker.AI.Application.Extraction;
using JobTracker.AI.Application.Summaries;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.AI.Application;

/// <summary>Registers the Application layer of the AI service.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Enabled = false;

        services.AddValidatorsFromAssemblyContaining<SummaryService>();
        services.AddScoped<ISummaryService, SummaryService>();
        services.AddScoped<IExtractionService, ExtractionService>();

        return services;
    }
}
