namespace JobTracker.Users.Application.Users.Dtos;

/// <summary>
/// The structured content of a professional profile — the same shape the AI produces from a CV,
/// plus certifications. Stored as opaque JSON on the <see cref="Domain.Entities.Profile"/> aggregate.
/// </summary>
public sealed record ProfileContentDto(
    string? FullName,
    string? Headline,
    string? Summary,
    string? Location,
    int? YearsOfExperience,
    IReadOnlyList<string> Skills,
    IReadOnlyList<ProfileExperienceDto> Experience,
    IReadOnlyList<ProfileEducationDto> Education,
    IReadOnlyList<ProfileCertificationDto> Certifications,
    IReadOnlyList<string> Links);

public sealed record ProfileExperienceDto(string Company, string? Title, string? Period, IReadOnlyList<string> Highlights);

public sealed record ProfileEducationDto(string Institution, string? Degree, string? Year);

public sealed record ProfileCertificationDto(string Name, string? Issuer, string? Year);
