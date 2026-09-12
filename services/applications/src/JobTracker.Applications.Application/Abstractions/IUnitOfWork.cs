namespace JobTracker.Applications.Application.Abstractions;

/// <summary>
/// Commits the changes made within a single business operation. Keeping this separate from the
/// repository draws a clean line between "collecting changes" (the repository) and "persisting
/// them" (the unit of work), and gives each use case one explicit place to decide when its work
/// is saved. In this service it is implemented by the EF Core <c>DbContext</c>.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
