using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Assistant.Dtos;
using JobTracker.AI.Application.Exceptions;
using JobTracker.AI.Domain.ValueObjects;

namespace JobTracker.AI.Application.Assistant;

/// <summary>
/// The application assistant use cases. Each frames a system prompt, hands the candidate profile +
/// job description to the <see cref="IAiCompletionClient"/> port, and shapes the result. Cover
/// letters and résumés come back as text; the fit assessment is strict JSON parsed here (same
/// pattern as extraction). The candidate profile is never required by the API, but every use case
/// is far more useful with one — the caller supplies the active profile.
/// </summary>
public sealed class AssistantService : IAssistantService
{
    private const string CoverLetterPrompt =
        "You write a concise, professional cover letter for a candidate applying to a specific role, " +
        "using the CANDIDATE PROFILE and JOB DESCRIPTION below. Keep it to 250–350 words in 3–4 short " +
        "paragraphs — confident but genuine — referencing concrete details from both the posting and the " +
        "candidate's real experience. Never invent facts that are not in the profile. Return ONLY the " +
        "letter body text: no preamble, no markdown, no bracketed placeholders.";

    private const string TailoredCvPrompt =
        "You produce a tailored resume for a candidate applying to a specific role. Using ONLY facts from " +
        "the CANDIDATE PROFILE, write a one-page resume in Markdown tailored to the JOB DESCRIPTION: a short " +
        "professional summary aimed at this role, a skills line that leads with the skills the posting asks " +
        "for, and experience entries whose bullet points foreground the most relevant achievements. Reorder " +
        "and rephrase for relevance, but never fabricate experience, skills, employers, or dates. Return " +
        "ONLY Markdown.";

    private const string FitPrompt =
        "You produce a fit assessment: how well a candidate matches a specific job. Return ONLY a JSON " +
        "object — no markdown fences, no commentary — with exactly these keys: \"score\" (integer 0-100, the " +
        "overall match), \"strengths\" (array of short strings where the candidate clearly matches), \"gaps\" " +
        "(array of short strings for requirements the candidate lacks or that are unclear), and \"summary\" " +
        "(one or two sentences of honest overall assessment). Judge strictly from the CANDIDATE PROFILE " +
        "against the JOB DESCRIPTION; never invent qualifications.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private readonly IAiCompletionClient _ai;
    private readonly IValidator<AssistRequest> _validator;

    public AssistantService(IAiCompletionClient ai, IValidator<AssistRequest> validator)
    {
        _ai = ai;
        _validator = validator;
    }

    public async Task<CoverLetterResponse> CoverLetterAsync(AssistRequest request, CancellationToken cancellationToken = default)
    {
        var completion = await CompleteAsync(CoverLetterPrompt, request, cancellationToken);
        return new CoverLetterResponse(completion.Text.Trim(), completion.Model);
    }

    public async Task<TailoredCvResponse> TailoredCvAsync(AssistRequest request, CancellationToken cancellationToken = default)
    {
        var completion = await CompleteAsync(TailoredCvPrompt, request, cancellationToken);
        return new TailoredCvResponse(completion.Text.Trim(), completion.Model);
    }

    public async Task<FitResponse> FitAsync(AssistRequest request, CancellationToken cancellationToken = default)
    {
        var completion = await CompleteAsync(FitPrompt, request, cancellationToken);
        var fit = ParseFit(completion.Text);

        return new FitResponse(
            Math.Clamp(fit.Score ?? 0, 0, 100),
            fit.Strengths ?? Array.Empty<string>(),
            fit.Gaps ?? Array.Empty<string>(),
            fit.Summary?.Trim() ?? string.Empty,
            completion.Model);
    }

    private async Task<AiCompletion> CompleteAsync(string systemPrompt, AssistRequest request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var jobDescription = JobDescription.Create(request.JobDescription);

        var userPrompt = string.IsNullOrWhiteSpace(request.CandidateProfile)
            ? jobDescription.Value
            : $"CANDIDATE PROFILE:\n{request.CandidateProfile.Trim()}\n\nJOB DESCRIPTION:\n{jobDescription.Value}";

        return await _ai.CompleteAsync(systemPrompt, userPrompt, cancellationToken);
    }

    private static ParsedFit ParseFit(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new AiUnavailableException("The AI did not return a fit assessment.");
        }

        try
        {
            return JsonSerializer.Deserialize<ParsedFit>(text[start..(end + 1)], JsonOptions) ?? EmptyFit;
        }
        catch (JsonException)
        {
            throw new AiUnavailableException("The AI returned an unexpected format. Please try again.");
        }
    }

    private static readonly ParsedFit EmptyFit = new(null, null, null, null);

    private sealed record ParsedFit(
        int? Score,
        IReadOnlyList<string>? Strengths,
        IReadOnlyList<string>? Gaps,
        string? Summary);
}
