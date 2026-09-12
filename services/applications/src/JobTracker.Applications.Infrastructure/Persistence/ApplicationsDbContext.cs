using System.Reflection;
using JobTracker.Applications.Application.Abstractions;
using JobTracker.Applications.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Applications.Infrastructure.Persistence;

/// <summary>
/// The EF Core database context for the Applications service.
/// </summary>
/// <remarks>
/// It also implements <see cref="IUnitOfWork"/> — and needs no extra code to do so, because a
/// <see cref="DbContext"/> already exposes <c>SaveChangesAsync</c>. This is the pragmatic reading
/// of the Unit of Work pattern: EF's context <i>is</i> a unit of work (it tracks changes and
/// commits them atomically), so we expose it to the Application layer through our own interface
/// rather than inventing a second abstraction.
/// </remarks>
public sealed class ApplicationsDbContext : DbContext, IUnitOfWork
{
    public ApplicationsDbContext(DbContextOptions<ApplicationsDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Pick up every IEntityTypeConfiguration<T> defined in this assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
