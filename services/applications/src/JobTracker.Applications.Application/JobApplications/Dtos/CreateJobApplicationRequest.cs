using JobTracker.Applications.Domain.Enums;

namespace JobTracker.Applications.Application.JobApplications.Dtos;

/// <summary>
/// Payload to create a job application. <c>Id</c>, <c>CreatedAt</c> and <c>UpdatedAt</c> are
/// server-assigned and therefore deliberately absent — a client cannot set them (no over-posting).
/// </summary>
public sealed record CreateJobApplicationRequest(
    string Company,
    string Position,
    ApplicationStatus Status,
    ApplicationSource Source,
    DateOnly? AppliedDate,
    string? Notes,
    SalaryRangeDto? Salary);
