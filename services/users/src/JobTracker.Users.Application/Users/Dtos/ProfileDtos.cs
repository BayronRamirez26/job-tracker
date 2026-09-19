namespace JobTracker.Users.Application.Users.Dtos;

/// <summary>A lightweight row for the profiles list — no content, just what the picker needs.</summary>
public sealed record ProfileSummaryDto(Guid Id, string Name, DateTimeOffset UpdatedAt);

/// <summary>A full profile: its identity, name, timestamps, and structured content.</summary>
public sealed record ProfileDetailDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    ProfileContentDto Content);

/// <summary>The request body for creating or replacing a profile.</summary>
public sealed record SaveProfileRequest(string Name, ProfileContentDto Content);
