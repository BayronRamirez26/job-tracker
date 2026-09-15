using JobTracker.Users.Application.Users.Dtos;

namespace JobTracker.Users.Application.Users;

/// <summary>User queries (e.g. resolving the current user for the /me endpoint).</summary>
public interface IUserService
{
    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
