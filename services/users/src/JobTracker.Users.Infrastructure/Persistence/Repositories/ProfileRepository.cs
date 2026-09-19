using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Users.Infrastructure.Persistence.Repositories;

internal sealed class ProfileRepository : IProfileRepository
{
    private readonly UsersDbContext _context;

    public ProfileRepository(UsersDbContext context)
    {
        _context = context;
    }

    public async Task<Profile?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
        => await _context.Profiles.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Profile>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Profiles
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Profile profile, CancellationToken cancellationToken = default)
        => await _context.Profiles.AddAsync(profile, cancellationToken);

    public void Remove(Profile profile) => _context.Profiles.Remove(profile);
}
