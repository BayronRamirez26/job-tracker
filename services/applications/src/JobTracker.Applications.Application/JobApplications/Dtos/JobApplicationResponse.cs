using JobTracker.Applications.Domain.Enums;

namespace JobTracker.Applications.Application.JobApplications.Dtos;

/// <summary>The representation of a job application returned to clients.</summary>
public sealed record JobApplicationResponse(
    Guid Id,
    string Company,
    string Position,
    ApplicationStatus Status,
    ApplicationSource Source,
    DateOnly? AppliedDate,
    string? Notes,
    SalaryRangeDto? Salary,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? JobDescription = null);
