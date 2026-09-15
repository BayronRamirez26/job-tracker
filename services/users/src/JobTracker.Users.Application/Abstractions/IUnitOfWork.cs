namespace JobTracker.Users.Application.Abstractions;

/// <summary>Commits the changes made in one business operation (implemented by the EF DbContext).</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
