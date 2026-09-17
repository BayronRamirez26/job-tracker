using Anthropic;
using Anthropic.Models.Messages;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Exceptions;
using Microsoft.Extensions.Options;

namespace JobTracker.AI.Infrastructure.Ai;

/// <summary>
/// Implements the AI port against the official Anthropic (Claude) SDK. Any provider failure is
/// wrapped in <see cref="AiUnavailableException"/> so the API can answer 503 rather than leaking a
/// raw SDK exception as a 500.
/// </summary>
internal sealed class AnthropicCompletionClient : IAiCompletionClient
{
    private readonly AnthropicClient _client;
    private readonly AnthropicOptions _options;

    public AnthropicCompletionClient(IOptions<AnthropicOptions> options)
    {
        _options = options.Value;
        _client = new AnthropicClient { ApiKey = _options.ApiKey };
    }

    public async Task<AiCompletion> CompleteAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.Messages.Create(new MessageCreateParams
            {
                Model = _options.Model,
                MaxTokens = _options.MaxTokens,
                System = systemPrompt,
                OutputConfig = new OutputConfig { Effort = Effort.Low }, // a summary doesn't need deep reasoning
                Messages = [new() { Role = Role.User, Content = userPrompt }],
            });

            var text = string.Concat(
                response.Content
                    .Select(block => block.Value)
                    .OfType<TextBlock>()
                    .Select(block => block.Text));

            return new AiCompletion(text, _options.Model);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new AiUnavailableException("The AI provider request failed.", ex);
        }
    }
}
