namespace JobTracker.AI.Application.Cv.Dtos;

/// <summary>One education entry parsed from a CV.</summary>
public sealed record CvEducation(string Institution, string? Degree, string? Year);
