namespace JobTracker.Users.Application.Users.Dtos;

/// <summary>Public representation of a user. Note: never includes the password hash.</summary>
public sealed record UserResponse(Guid Id, string Email, string DisplayName, DateTimeOffset CreatedAt);
