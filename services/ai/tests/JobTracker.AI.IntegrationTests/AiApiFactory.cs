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
        public const string StubProfile =
            "{\"fullName\":\"Ada Lovelace\",\"headline\":\"Software Engineer\",\"summary\":\"Builds things.\"," +
            "\"location\":\"Remote\",\"yearsOfExperience\":6,\"skills\":[\"C#\",\".NET\"]," +
            "\"experience\":[{\"company\":\"Acme\",\"title\":\"Engineer\",\"period\":\"2020-2023\",\"highlights\":[\"Shipped\"]}]," +
            "\"education\":[{\"institution\":\"MIT\",\"degree\":\"BSc\",\"year\":\"2016\"}],\"links\":[]}";
        public const string StubFit =
            "{\"score\":78,\"strengths\":[\"Strong C# background\"],\"gaps\":[\"No Kubernetes\"],\"summary\":\"A solid overall fit.\"}";
        public const string StubCoverLetter = "Dear Hiring Manager, I am excited to apply. Sincerely, Ada.";
        public const string StubTailoredCv = "# Ada Lovelace\n\n## Summary\nTailored to the role.";
        public const string StubLatex = "\\documentclass{article}\n\\begin{document}\nTailored.\n\\end{document}";

        // Each use case names its intent in the system prompt; return the shape it expects.
        public Task<AiCompletion> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            string text;
            if (systemPrompt.Contains("professional profile", StringComparison.OrdinalIgnoreCase))
            {
                text = StubProfile;
            }
            else if (systemPrompt.Contains("job posting", StringComparison.OrdinalIgnoreCase))
            {
                text = StubExtraction;
            }
            else if (systemPrompt.Contains("fit assessment", StringComparison.OrdinalIgnoreCase))
            {
                text = StubFit;
            }
            else if (systemPrompt.Contains("cover letter", StringComparison.OrdinalIgnoreCase))
            {
                text = StubCoverLetter;
            }
            else if (systemPrompt.Contains("latex", StringComparison.OrdinalIgnoreCase))
            {
                text = StubLatex;
            }
            else if (systemPrompt.Contains("tailored resume", StringComparison.OrdinalIgnoreCase))
            {
                text = StubTailoredCv;
            }
            else
            {
                text = StubSummary;
            }

            return Task.FromResult(new AiCompletion(text, "stub-model"));
        }
    }
}
