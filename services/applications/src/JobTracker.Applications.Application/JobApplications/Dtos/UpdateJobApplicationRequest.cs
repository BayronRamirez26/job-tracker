using JobTracker.Applications.Domain.Enums;

namespace JobTracker.Applications.Application.JobApplications.Dtos;

/// <summary>
/// Payload to replace a job application's editable fields (a full update / PUT). The id comes
/// from the route, not the body.
/// </summary>
public sealed record UpdateJobApplicationRequest(
    string Company,
    string Position,
    ApplicationStatus Status,
    ApplicationSource Source,
    DateOnly? AppliedDate,
    string? Notes,
    SalaryRangeDto? Salary,
    string? JobDescription = null);
