using JobTracker.Users.Domain.Entities;
using JobTracker.Users.Domain.ValueObjects;

namespace JobTracker.Users.Application.Abstractions;

/// <summary>Port for persisting and querying <see cref="User"/> aggregates.</summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
