namespace JobTracker.AI.Application.Cv.Dtos;

/// <summary>Payload to build a professional profile from a CV (plain text or LaTeX source).</summary>
public sealed record ParseCvRequest(string Cv);
