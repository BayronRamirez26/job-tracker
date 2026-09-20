namespace JobTracker.AI.Application.Assistant.Dtos;

/// <summary>A generated cover letter and the model that produced it.</summary>
public sealed record CoverLetterResponse(string Letter, string Model);

/// <summary>
/// A résumé tailored to the posting. <c>Format</c> is "markdown" or "latex"; <c>Content</c> holds
/// the résumé in that format.
/// </summary>
public sealed record TailoredCvResponse(string Content, string Format, string Model);

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
