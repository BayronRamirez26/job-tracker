using JobTracker.Users.Domain.Entities;

namespace JobTracker.Users.Application.Abstractions;

/// <summary>Port for persisting and querying <see cref="Profile"/> aggregates, always scoped to their owner.</summary>
public interface IProfileRepository
{
    Task<Profile?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Profile>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Profile profile, CancellationToken cancellationToken = default);

    void Remove(Profile profile);
}
