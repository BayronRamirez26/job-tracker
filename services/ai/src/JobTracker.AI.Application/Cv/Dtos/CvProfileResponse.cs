namespace JobTracker.AI.Application.Cv.Dtos;

/// <summary>
/// A candidate's professional profile, built from their CV. Scalar fields may be null and the
/// lists may be empty when the CV does not state them.
/// </summary>
public sealed record CvProfileResponse(
    string? FullName,
    string? Headline,
    string? Summary,
    string? Location,
    int? YearsOfExperience,
    IReadOnlyList<string> Skills,
    IReadOnlyList<CvExperience> Experience,
    IReadOnlyList<CvEducation> Education,
    IReadOnlyList<CvCertification> Certifications,
    IReadOnlyList<string> Links,
    string Model);
