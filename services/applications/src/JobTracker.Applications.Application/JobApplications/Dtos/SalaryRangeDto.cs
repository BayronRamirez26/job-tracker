namespace JobTracker.Applications.Application.JobApplications.Dtos;

/// <summary>
/// Salary band as exchanged over the API. It mirrors the <c>SalaryRange</c> value object but is
/// a separate type: DTOs are the public contract and are deliberately decoupled from the domain
/// model so the two can evolve independently.
/// </summary>
public sealed record SalaryRangeDto(decimal Min, decimal Max, string Currency);
