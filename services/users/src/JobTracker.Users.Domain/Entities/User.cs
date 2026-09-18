using JobTracker.Users.Domain.Exceptions;
using JobTracker.Users.Domain.ValueObjects;

namespace JobTracker.Users.Domain.Entities;

/// <summary>
/// A registered user and aggregate root. Note what it deliberately does NOT know: how passwords
/// are hashed. It stores an opaque <see cref="PasswordHash"/> string handed to it by the
/// Application layer, keeping all cryptography out of the domain (an infrastructure concern).
/// Plaintext passwords never reach this class.
/// </summary>
public sealed class User
{
    private User()
    {
    }

    private User(Guid id, Email email, string passwordHash, string displayName, DateTimeOffset createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        DisplayName = displayName;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public Email Email { get; private set; } = null!;

    /// <summary>An opaque password hash produced by the Application/Infrastructure layers.</summary>
    public string PasswordHash { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// The user's professional profile as opaque JSON. Like <see cref="PasswordHash"/>, the domain
    /// holds it without knowing its shape — the Application layer owns (de)serialization. Null until
    /// the user builds a profile from their CV.
    /// </summary>
    public string? ProfileJson { get; private set; }

    public static User Create(Email email, string passwordHash, string displayName)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash is required.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new DomainException("Display name is required.");
        }

        return new User(Guid.NewGuid(), email, passwordHash, displayName.Trim(), DateTimeOffset.UtcNow);
    }

    public void SetProfile(string? profileJson)
    {
        ProfileJson = string.IsNullOrWhiteSpace(profileJson) ? null : profileJson;
    }
}
