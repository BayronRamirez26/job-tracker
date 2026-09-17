using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Infrastructure.Ai;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracker.AI.Infrastructure;

/// <summary>Registers the Infrastructure layer: Anthropic settings + the completion client.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AnthropicOptions>(configuration.GetSection(AnthropicOptions.SectionName));

        // The Anthropic SDK client is thread-safe; register it as a singleton.
        services.AddSingleton<IAiCompletionClient, AnthropicCompletionClient>();

        return services;
    }
}
