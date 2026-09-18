namespace JobTracker.AI.Infrastructure.Ai;

/// <summary>
/// Anthropic settings bound from the "Anthropic" configuration section. Model/MaxTokens are
/// non-secret (appsettings); ApiKey is a real secret and comes from user-secrets or the
/// Anthropic__ApiKey environment variable — never a committed file.
/// </summary>
public sealed class AnthropicOptions
{
    public const string SectionName = "Anthropic";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-opus-5";

    public int MaxTokens { get; set; } = 2048;
}
