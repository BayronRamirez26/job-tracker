using JobTracker.Applications.Application.Abstractions;
using JobTracker.Applications.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Applications.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IJobApplicationRepository"/>. Kept deliberately thin: it
/// does not leak <c>IQueryable</c> to callers, so query shape stays an infrastructure detail.
/// The owned <c>SalaryRange</c> is loaded automatically with the aggregate.
/// </summary>
internal sealed class JobApplicationRepository : IJobApplicationRepository
{
    private readonly ApplicationsDbContext _context;

    public JobApplicationRepository(ApplicationsDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<JobApplication>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.JobApplications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(JobApplication application, CancellationToken cancellationToken = default)
        => await _context.JobApplications.AddAsync(application, cancellationToken);

    public void Remove(JobApplication application)
        => _context.JobApplications.Remove(application);
}
