namespace JobTracker.AI.Application.Summaries.Dtos;

/// <summary>
/// Payload to summarize a job description. When <paramref name="CandidateProfile"/> is supplied
/// (a compact professional profile), the summary is tailored to that candidate.
/// </summary>
public sealed record SummarizeRequest(string JobDescription, string? CandidateProfile = null);
