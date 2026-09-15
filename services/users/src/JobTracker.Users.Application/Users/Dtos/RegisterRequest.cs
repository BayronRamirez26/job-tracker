namespace JobTracker.Users.Application.Users.Dtos;

/// <summary>Payload to register a new account. The plaintext password never leaves this layer.</summary>
public sealed record RegisterRequest(string Email, string Password, string DisplayName);
