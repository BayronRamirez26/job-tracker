namespace JobTracker.AI.Application.Assistant.Dtos;

/// <summary>
/// Shared input for the application assistant: the job posting, plus the candidate's professional
/// profile (a compact text block). All three assistant use cases work from this pair. <c>Format</c>
/// is only used by the tailored-CV use case — "latex" produces LaTeX, anything else Markdown.
/// </summary>
public sealed record AssistRequest(string JobDescription, string? CandidateProfile = null, string? Format = null);
