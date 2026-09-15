namespace JobTracker.Users.Infrastructure.Security;

/// <summary>
/// JWT settings bound from the "Jwt" configuration section. Issuer/Audience/ExpiryMinutes are
/// non-secret (appsettings); the SigningKey is a secret (user-secrets / environment variable).
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
