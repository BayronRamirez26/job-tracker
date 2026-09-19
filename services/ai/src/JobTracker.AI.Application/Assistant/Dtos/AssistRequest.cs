namespace JobTracker.AI.Application.Assistant.Dtos;

/// <summary>
/// Shared input for the application assistant: the job posting, plus the candidate's professional
/// profile (a compact text block). All three assistant use cases work from this pair.
/// </summary>
public sealed record AssistRequest(string JobDescription, string? CandidateProfile = null);
