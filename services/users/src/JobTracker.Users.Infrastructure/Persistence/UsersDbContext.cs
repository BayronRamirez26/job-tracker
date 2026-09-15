using System.Reflection;
using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Users.Infrastructure.Persistence;

/// <summary>EF Core context for the Users service; also serves as the unit of work.</summary>
public sealed class UsersDbContext : DbContext, IUnitOfWork
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
