using JobTracker.Applications.Domain.Exceptions;

namespace JobTracker.Applications.Domain.ValueObjects;

/// <summary>
/// The salary band advertised or expected for a role, e.g. 90,000–120,000 USD.
/// </summary>
/// <remarks>
/// This is a <b>value object</b>: it has no identity of its own, and two ranges with the same
/// min, max, and currency are considered equal. Declaring it as a C# <c>record</c> gives us
/// that value-based equality (and a readable <c>ToString</c>) for free. It is also immutable
/// and self-validating — construction goes through <see cref="Create"/>, so an invalid range
/// simply cannot exist. That combination (no identity, compared by value, immutable) is the
/// defining contrast with an <i>entity</i> such as the JobApplication aggregate, which is
/// tracked by its identity as it changes over time.
/// </remarks>
public sealed record SalaryRange
{
    // Private so all construction is funnelled through the validating Create factory.
    private SalaryRange(decimal min, decimal max, string currency)
    {
        Min = min;
        Max = max;
        Currency = currency;
    }

    public decimal Min { get; }

    public decimal Max { get; }

    /// <summary>ISO 4217 currency code, stored upper-cased, e.g. "USD", "EUR", "CRC".</summary>
    public string Currency { get; }

    /// <summary>Creates a valid salary range or throws <see cref="DomainException"/>.</summary>
    public static SalaryRange Create(decimal min, decimal max, string currency)
    {
        if (min < 0)
        {
            throw new DomainException("Salary minimum cannot be negative.");
        }

        if (max < min)
        {
            throw new DomainException("Salary maximum cannot be less than the minimum.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Salary currency is required.");
        }

        var normalized = currency.Trim().ToUpperInvariant();
        if (normalized.Length != 3)
        {
            throw new DomainException("Salary currency must be a 3-letter ISO 4217 code (e.g. USD).");
        }

        return new SalaryRange(min, max, normalized);
    }
}
