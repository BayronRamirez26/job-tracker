using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Summaries.Dtos;
using JobTracker.AI.Domain.ValueObjects;

namespace JobTracker.AI.Application.Summaries;

/// <summary>
/// Validates the input, frames the prompt, and delegates the actual generation to the
/// <see cref="IAiCompletionClient"/> port. All of the "how do we talk to Claude" detail lives in
/// Infrastructure; this class only owns the use case.
/// </summary>
public sealed class SummaryService : ISummaryService
{
    private const string SystemPrompt =
        "You summarize job descriptions for a candidate tracking their applications. " +
        "Produce 3–5 short bullet points covering: the company and role, the key responsibilities, " +
        "the must-have requirements, and any notable perks or red flags. Be factual and concise; " +
        "never invent details that are not present in the text. " +
        "If a CANDIDATE PROFILE is provided, tailor the summary to that candidate: highlight where " +
        "the role fits their background and call out any notable gaps.";

    private readonly IAiCompletionClient _aiCompletionClient;
    private readonly IValidator<SummarizeRequest> _validator;

    public SummaryService(IAiCompletionClient aiCompletionClient, IValidator<SummarizeRequest> validator)
    {
        _aiCompletionClient = aiCompletionClient;
        _validator = validator;
    }

    public async Task<SummaryResponse> SummarizeAsync(SummarizeRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var jobDescription = JobDescription.Create(request.JobDescription);

        var userPrompt = BuildUserPrompt(jobDescription.Value, request.CandidateProfile);

        var completion = await _aiCompletionClient.CompleteAsync(SystemPrompt, userPrompt, cancellationToken);

        return new SummaryResponse(completion.Text, completion.Model);
    }

    /// <summary>Frames the job description, prefixed with the candidate profile when one is given.</summary>
    private static string BuildUserPrompt(string jobDescription, string? candidateProfile)
    {
        return string.IsNullOrWhiteSpace(candidateProfile)
            ? jobDescription
            : $"CANDIDATE PROFILE:\n{candidateProfile.Trim()}\n\nJOB DESCRIPTION:\n{jobDescription}";
    }
}
