using JobTracker.Users.Domain.Entities;

namespace JobTracker.Users.Application.Abstractions;

/// <summary>Port for issuing an access token (a signed JWT) for an authenticated user.</summary>
public interface ITokenGenerator
{
    AccessToken Generate(User user);
}

/// <summary>The issued token and when it expires.</summary>
public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);
