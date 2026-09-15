namespace JobTracker.Users.Application.Users.Dtos;

/// <summary>The result of a successful login: a signed JWT and its expiry.</summary>
public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAt);
