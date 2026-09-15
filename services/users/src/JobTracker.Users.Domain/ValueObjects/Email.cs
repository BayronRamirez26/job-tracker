using JobTracker.Users.Domain.Exceptions;

namespace JobTracker.Users.Domain.ValueObjects;

/// <summary>
/// A user's email address — a value object (compared by value, immutable, self-validating), the
/// auth-service sibling of the Applications service's SalaryRange. It normalizes to trimmed
/// lower-case so uniqueness and lookups are case-insensitive.
/// </summary>
/// <remarks>
/// Email validation is famously impossible to get perfect with a regex; we do a pragmatic
/// structural check. Real systems confirm ownership with a verification email (out of scope here).
/// </remarks>
public sealed record Email
{
    private Email(string value) => Value = value;

    public string Value { get; }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Email is required.");
        }

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 256 || !IsStructurallyValid(normalized))
        {
            throw new DomainException("Email is not a valid email address.");
        }

        return new Email(normalized);
    }

    private static bool IsStructurallyValid(string value)
    {
        var at = value.IndexOf('@');
        if (at <= 0 || at != value.LastIndexOf('@') || at == value.Length - 1)
        {
            return false; // needs exactly one '@' with a non-empty local part and something after
        }

        var domain = value[(at + 1)..];
        return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
    }

    public override string ToString() => Value;
}
