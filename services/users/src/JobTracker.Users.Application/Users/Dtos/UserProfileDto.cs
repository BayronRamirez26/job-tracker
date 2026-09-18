namespace JobTracker.Users.Application.Users.Dtos;

/// <summary>
/// A user's professional profile, built from their CV. Stored as JSON on the user; this record is
/// the public contract for GET/PUT /api/users/me/profile.
/// </summary>
public sealed record UserProfileDto(
    string? FullName,
    string? Headline,
    string? Summary,
    string? Location,
    int? YearsOfExperience,
    IReadOnlyList<string> Skills,
    IReadOnlyList<ProfileExperienceDto> Experience,
    IReadOnlyList<ProfileEducationDto> Education,
    IReadOnlyList<string> Links);

public sealed record ProfileExperienceDto(string Company, string? Title, string? Period, IReadOnlyList<string> Highlights);

public sealed record ProfileEducationDto(string Institution, string? Degree, string? Year);
