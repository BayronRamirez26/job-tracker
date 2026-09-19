namespace JobTracker.AI.Application.Assistant.Dtos;

/// <summary>A generated cover letter and the model that produced it.</summary>
public sealed record CoverLetterResponse(string Letter, string Model);

/// <summary>A résumé tailored to the posting, as Markdown, and the model that produced it.</summary>
public sealed record TailoredCvResponse(string Markdown, string Model);

/// <summary>
/// A fit assessment of the candidate against the posting: an overall 0–100 score, where they match,
/// where they fall short, and a short verdict.
/// </summary>
public sealed record FitResponse(
    int Score,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Gaps,
    string Summary,
    string Model);
