namespace JobTracker.AI.Application.Extraction.Dtos;

/// <summary>Payload to extract structured application fields from a job description.</summary>
public sealed record ExtractRequest(string JobDescription);
