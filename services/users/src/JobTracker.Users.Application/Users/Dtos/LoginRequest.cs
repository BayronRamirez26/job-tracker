namespace JobTracker.Users.Application.Users.Dtos;

/// <summary>Credentials to exchange for an access token.</summary>
public sealed record LoginRequest(string Email, string Password);
