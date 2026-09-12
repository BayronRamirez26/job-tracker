using JobTracker.Applications.Domain.Entities;

namespace JobTracker.Applications.Application.Abstractions;

/// <summary>
/// Port for persisting <see cref="JobApplication"/> aggregates. It is declared here, in the
/// Application layer, and implemented in Infrastructure — the dependency-inversion seam that
/// keeps this layer (and the Domain) completely free of Entity Framework Core.
/// </summary>
/// <remarks>
/// There is intentionally no <c>Update</c> method. The workflow is: load an aggregate, mutate
/// it through its own methods, then commit via <see cref="IUnitOfWork"/>. The persistence
/// layer's change tracking detects the modifications, so an explicit "update" call would be
/// redundant.
/// </remarks>
public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobApplication>> ListAsync(CancellationToken cancellationToken = default);

    Task AddAsync(JobApplication application, CancellationToken cancellationToken = default);

    void Remove(JobApplication application);
}
