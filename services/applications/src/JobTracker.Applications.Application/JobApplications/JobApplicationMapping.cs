using JobTracker.Applications.Application.JobApplications.Dtos;
using JobTracker.Applications.Domain.Entities;
using JobTracker.Applications.Domain.ValueObjects;

namespace JobTracker.Applications.Application.JobApplications;

/// <summary>
/// Hand-written mapping between domain objects and DTOs. We map by hand rather than reaching for
/// a tool like AutoMapper: it is explicit, trivially debuggable, refactor-safe (rename a property
/// and the compiler points at the mapping), and carries no reflection cost or hidden config. The
/// trade-off is a little boilerplate, which is negligible at this scale.
/// </summary>
internal static class JobApplicationMapping
{
    public static JobApplicationResponse ToResponse(this JobApplication entity)
        => new(
            entity.Id,
            entity.Company,
            entity.Position,
            entity.Status,
            entity.Source,
            entity.AppliedDate,
            entity.Notes,
            entity.Salary is null
                ? null
                : new SalaryRangeDto(entity.Salary.Min, entity.Salary.Max, entity.Salary.Currency),
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.JobDescription);

    /// <summary>
    /// Converts the salary DTO into the domain value object. <c>SalaryRange.Create</c> re-checks
    /// the invariants; by this point validation has already run, so this is a defensive guard.
    /// </summary>
    public static SalaryRange? ToDomain(this SalaryRangeDto? dto)
        => dto is null ? null : SalaryRange.Create(dto.Min, dto.Max, dto.Currency);
}
