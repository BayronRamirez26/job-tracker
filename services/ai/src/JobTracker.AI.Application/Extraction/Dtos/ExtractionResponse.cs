namespace JobTracker.AI.Application.Extraction.Dtos;

/// <summary>
/// Structured fields extracted from a job description. Any field may be null when the posting
/// does not clearly state it — the client fills what it can and leaves the rest to the user.
/// </summary>
public sealed record ExtractionResponse(
    string? Company,
    string? Position,
    ExtractedSalary? Salary,
    string? Notes,
    string Model);
