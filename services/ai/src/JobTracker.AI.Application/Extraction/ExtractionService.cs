using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Exceptions;
using JobTracker.AI.Application.Extraction.Dtos;
using JobTracker.AI.Domain.ValueObjects;

namespace JobTracker.AI.Application.Extraction;

/// <summary>
/// Turns a free-text job posting into structured application fields. It asks the model for a
/// strict JSON object and parses it here; the "how do we talk to Claude" detail stays in
/// Infrastructure behind <see cref="IAiCompletionClient"/>.
/// </summary>
public sealed class ExtractionService : IExtractionService
{
    private const string SystemPrompt =
        "You extract structured data from a job posting for a candidate's application tracker. " +
        "Return ONLY a JSON object — no markdown fences, no commentary — with exactly these keys: " +
        "\"company\" (the hiring company's name, or null), " +
        "\"position\" (the job title, or null), " +
        "\"salary\" (an object {\"min\": number, \"max\": number, \"currency\": a 3-letter ISO 4217 code} " +
        "when a pay range is stated, otherwise null), and " +
        "\"notes\" (one short sentence — if a CANDIDATE PROFILE is provided, note how the role fits " +
        "that candidate; otherwise summarise the key requirements — or null). " +
        "Use null for anything not clearly stated. Never invent values.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private readonly IAiCompletionClient _aiCompletionClient;
    private readonly IValidator<ExtractRequest> _validator;

    public ExtractionService(IAiCompletionClient aiCompletionClient, IValidator<ExtractRequest> validator)
    {
        _aiCompletionClient = aiCompletionClient;
        _validator = validator;
    }

    public async Task<ExtractionResponse> ExtractAsync(ExtractRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var jobDescription = JobDescription.Create(request.JobDescription);

        var userPrompt = string.IsNullOrWhiteSpace(request.CandidateProfile)
            ? jobDescription.Value
            : $"CANDIDATE PROFILE:\n{request.CandidateProfile.Trim()}\n\nJOB DESCRIPTION:\n{jobDescription.Value}";

        var completion = await _aiCompletionClient.CompleteAsync(SystemPrompt, userPrompt, cancellationToken);

        var fields = Parse(completion.Text);

        return new ExtractionResponse(
            NullIfBlank(fields.Company),
            NullIfBlank(fields.Position),
            fields.Salary,
            NullIfBlank(fields.Notes),
            completion.Model);
    }

    private static ParsedFields Parse(string text)
    {
        var json = IsolateJsonObject(text);
        try
        {
            return JsonSerializer.Deserialize<ParsedFields>(json, JsonOptions) ?? Empty;
        }
        catch (JsonException)
        {
            throw new AiUnavailableException("The AI returned an unexpected format. Please try again.");
        }
    }

    /// <summary>Grabs the outermost { ... } so a stray markdown fence or prose can't break parsing.</summary>
    private static string IsolateJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new AiUnavailableException("The AI did not return any structured data.");
        }

        return text[start..(end + 1)];
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static readonly ParsedFields Empty = new(null, null, null, null);

    private sealed record ParsedFields(string? Company, string? Position, ExtractedSalary? Salary, string? Notes);
}
