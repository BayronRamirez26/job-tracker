using JobTracker.Applications.Domain.Enums;
using JobTracker.Applications.Domain.Exceptions;
using JobTracker.Applications.Domain.ValueObjects;

namespace JobTracker.Applications.Domain.Entities;

/// <summary>
/// A single job application being tracked. This is the <b>aggregate root</b> of the service:
/// every change is made through one of its methods, so its invariants can never be bypassed.
/// </summary>
/// <remarks>
/// The model is deliberately "rich": each property has a private setter and all mutation flows
/// through intent-revealing methods (<see cref="Create"/>, <see cref="UpdateDetails"/>,
/// <see cref="ChangeStatus"/>). This makes invalid states unreachable — you cannot, for
/// instance, end up with a blank company name. The cost is a little more ceremony than an
/// "anemic" model (a plain bag of public get/set properties); we accept that in exchange for
/// the guarantees, and because it keeps business rules in the Domain instead of leaking into
/// controllers or services.
/// </remarks>
public sealed class JobApplication
{
    // Parameterless constructor used ONLY by EF Core to materialise entities read from the
    // database. Application code must go through the Create factory. EF assigns the values via
    // the (private) property setters / backing fields.
    private JobApplication()
    {
    }

    private JobApplication(
        Guid id,
        Guid userId,
        string company,
        string position,
        ApplicationStatus status,
        ApplicationSource source,
        DateOnly? appliedDate,
        string? notes,
        SalaryRange? salary,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        UserId = userId;
        Company = company;
        Position = position;
        Status = status;
        Source = source;
        AppliedDate = appliedDate;
        Notes = notes;
        Salary = salary;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public Guid Id { get; private set; }

    /// <summary>The owner of this application (the 'sub' of the authenticated user). Set once.</summary>
    public Guid UserId { get; private set; }

    public string Company { get; private set; } = null!;

    public string Position { get; private set; } = null!;

    public ApplicationStatus Status { get; private set; }

    public ApplicationSource Source { get; private set; }

    /// <summary>The date applied. Null while the application is still a <see cref="ApplicationStatus.Wishlist"/>.</summary>
    public DateOnly? AppliedDate { get; private set; }

    public string? Notes { get; private set; }

    public SalaryRange? Salary { get; private set; }

    /// <summary>When the record was created (UTC). Set once and never changed.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>When the record was last modified (UTC).</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Creates a new, valid job application owned by <paramref name="userId"/>.</summary>
    public static JobApplication Create(
        Guid userId,
        string company,
        string position,
        ApplicationStatus status,
        ApplicationSource source,
        DateOnly? appliedDate = null,
        string? notes = null,
        SalaryRange? salary = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("An owner is required.");
        }

        var now = DateTimeOffset.UtcNow;

        return new JobApplication(
            id: Guid.NewGuid(),
            userId: userId,
            company: NormalizeRequired(company, nameof(company)),
            position: NormalizeRequired(position, nameof(position)),
            status: status,
            source: source,
            appliedDate: appliedDate,
            notes: NormalizeOptional(notes),
            salary: salary,
            createdAt: now,
            updatedAt: now);
    }

    /// <summary>
    /// Updates the descriptive details of the application. Lifecycle changes go through
    /// <see cref="ChangeStatus"/> so that the two intents stay distinct and testable.
    /// </summary>
    public void UpdateDetails(
        string company,
        string position,
        ApplicationSource source,
        DateOnly? appliedDate,
        string? notes,
        SalaryRange? salary)
    {
        Company = NormalizeRequired(company, nameof(company));
        Position = NormalizeRequired(position, nameof(position));
        Source = source;
        AppliedDate = appliedDate;
        Notes = NormalizeOptional(notes);
        Salary = salary;
        Touch();
    }

    /// <summary>Moves the application to a new lifecycle stage.</summary>
    public void ChangeStatus(ApplicationStatus status)
    {
        Status = status;
        Touch();
    }

    // --- helpers -----------------------------------------------------------------------

    private void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    private static string NormalizeRequired(string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{Capitalize(field)} is required.");
        }

        return value.Trim();
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string Capitalize(string field)
        => field.Length == 0 ? field : char.ToUpperInvariant(field[0]) + field[1..];
}
