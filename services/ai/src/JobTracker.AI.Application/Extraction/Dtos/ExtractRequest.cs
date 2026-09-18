namespace JobTracker.AI.Application.Extraction.Dtos;

/// <summary>
/// Payload to extract structured application fields from a job description. When
/// <paramref name="CandidateProfile"/> is supplied, the notes field is framed around fit for
/// that candidate.
/// </summary>
public sealed record ExtractRequest(string JobDescription, string? CandidateProfile = null);
