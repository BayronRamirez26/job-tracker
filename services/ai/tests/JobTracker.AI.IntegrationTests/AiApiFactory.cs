using JobTracker.AI.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JobTracker.AI.IntegrationTests;

/// <summary>
/// Boots the real AI API but replaces the Claude client with a deterministic stub. The tests
/// exercise the HTTP pipeline, validation, and wiring end to end while making no real API call —
/// so they need no API key, cost nothing, and run in CI.
/// </summary>
public sealed class AiApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAiCompletionClient>();
            services.AddSingleton<IAiCompletionClient, StubAiCompletionClient>();
        });
    }

    private sealed class StubAiCompletionClient : IAiCompletionClient
    {
        public const string StubSummary = "• Stubbed summary bullet";
        public const string StubExtraction =
            "{\"company\":\"Acme\",\"position\":\"Backend Engineer\",\"salary\":null,\"notes\":\"Builds services.\"}";

        // The extraction prompt asks for JSON; summarize does not — return whatever that use case expects.
        public Task<AiCompletion> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            var text = systemPrompt.Contains("JSON", StringComparison.OrdinalIgnoreCase)
                ? StubExtraction
                : StubSummary;
            return Task.FromResult(new AiCompletion(text, "stub-model"));
        }
    }
}
