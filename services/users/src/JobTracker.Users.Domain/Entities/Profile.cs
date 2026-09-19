using JobTracker.Users.Domain.Exceptions;

namespace JobTracker.Users.Domain.Entities;

/// <summary>
/// A named professional profile owned by a user — e.g. "Full-Stack Developer" or "Sales Engineer".
/// A user can have several, and the AI features personalize to whichever one is chosen. Like
/// <see cref="User.PasswordHash"/>, the structured content is held as opaque JSON: the domain owns
/// the <see cref="Name"/> and ownership rules, while the Application layer owns the content's shape.
/// </summary>
public sealed class Profile
{
    private Profile()
    {
    }

    private Profile(Guid id, Guid userId, string name, string contentJson, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        Name = name;
        ContentJson = contentJson;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    /// <summary>The owning user. Profiles are always scoped to their owner.</summary>
    public Guid UserId { get; private set; }

    /// <summary>A human label the user gives the profile, e.g. "Full-Stack Developer".</summary>
    public string Name { get; private set; } = null!;

    /// <summary>The structured profile as opaque JSON; the Application layer (de)serializes it.</summary>
    public string ContentJson { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static Profile Create(Guid userId, string name, string? contentJson)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("An owner is required.");
        }

        return new Profile(Guid.NewGuid(), userId, NormalizeName(name), NormalizeContent(contentJson), DateTimeOffset.UtcNow);
    }

    public void Rename(string name)
    {
        Name = NormalizeName(name);
        Touch();
    }

    public void UpdateContent(string? contentJson)
    {
        ContentJson = NormalizeContent(contentJson);
        Touch();
    }

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A profile name is required.");
        }

        return name.Trim();
    }

    // An empty document is valid — a freshly created, not-yet-filled profile.
    private static string NormalizeContent(string? contentJson)
        => string.IsNullOrWhiteSpace(contentJson) ? "{}" : contentJson;
}
