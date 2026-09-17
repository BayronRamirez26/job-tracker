namespace JobTracker.AI.Application.Abstractions;

/// <summary>
/// Port for a large-language-model completion. Declared here so the Application layer can request
/// AI text without knowing which provider or SDK produces it; Infrastructure implements it against
/// the Anthropic (Claude) SDK. This seam is what keeps the SDK out of the business logic and lets
/// tests swap in a fake — no network, no API key, no cost.
/// </summary>
public interface IAiCompletionClient
{
    Task<AiCompletion> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default);
}

/// <summary>The model's text plus which model produced it (echoed back to callers for transparency).</summary>
public sealed record AiCompletion(string Text, string Model);
