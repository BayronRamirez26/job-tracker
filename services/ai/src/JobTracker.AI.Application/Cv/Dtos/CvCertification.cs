namespace JobTracker.AI.Application.Cv.Dtos;

/// <summary>One certification entry parsed from a CV.</summary>
public sealed record CvCertification(string Name, string? Issuer, string? Year);
