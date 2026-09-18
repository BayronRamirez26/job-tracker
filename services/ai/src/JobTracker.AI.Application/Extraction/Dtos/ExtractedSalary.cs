namespace JobTracker.AI.Application.Extraction.Dtos;

/// <summary>A pay range parsed from a posting. Matches the Applications service's salary contract.</summary>
public sealed record ExtractedSalary(decimal Min, decimal Max, string Currency);
