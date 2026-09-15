using JobTracker.Users.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace JobTracker.Users.Infrastructure.Security;

/// <summary>
/// PBKDF2 password hashing via ASP.NET Core Identity's standalone <c>PasswordHasher&lt;T&gt;</c> —
/// the vetted algorithm (salted, many iterations, versioned format) without pulling in the full
/// Identity system. We never implement our own crypto.
/// </summary>
internal sealed class PasswordHasher : IPasswordHasher
{
    // The generic TUser is unused by the default hashing implementation, so a shared placeholder
    // instance is fine.
    private static readonly object Placeholder = new();
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<object> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(Placeholder, password);

    public bool Verify(string passwordHash, string password)
    {
        var result = _hasher.VerifyHashedPassword(Placeholder, passwordHash, password);
        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
