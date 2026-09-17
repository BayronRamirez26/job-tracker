using JobTracker.AI.Domain.Exceptions;

namespace JobTracker.AI.Domain.ValueObjects;

/// <summary>
/// The raw job-description text to be summarized — a small value object that guarantees the input
/// is present and within a sane size before it is ever sent to the LLM. (This service is
/// stateless, so its "domain" is deliberately thin.)
/// </summary>
public sealed record JobDescription
{
    public const int MaxLength = 20_000;

    private JobDescription(string value) => Value = value;

    public string Value { get; }

    public static JobDescription Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Job description is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new DomainException($"Job description must be {MaxLength} characters or fewer.");
        }

        return new JobDescription(trimmed);
    }

    public override string ToString() => Value;
}
