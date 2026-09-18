using JobTracker.Users.Application.Users.Dtos;

namespace JobTracker.Users.Application.Users;

/// <summary>Reads and writes the current user's professional profile.</summary>
public interface IProfileService
{
    Task<UserProfileDto?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserProfileDto> SaveAsync(Guid userId, UserProfileDto profile, CancellationToken cancellationToken = default);
}
