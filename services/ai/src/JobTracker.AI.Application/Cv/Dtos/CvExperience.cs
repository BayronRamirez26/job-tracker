namespace JobTracker.AI.Application.Cv.Dtos;

/// <summary>One work-history entry parsed from a CV.</summary>
public sealed record CvExperience(string Company, string? Title, string? Period, IReadOnlyList<string> Highlights);
