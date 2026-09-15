using JobTracker.Users.Application.Users.Dtos;
using JobTracker.Users.Domain.Entities;

namespace JobTracker.Users.Application.Users;

/// <summary>Hand-written mapping from the User aggregate to its public DTO (never exposes the hash).</summary>
internal static class UserMapping
{
    public static UserResponse ToResponse(this User user)
        => new(user.Id, user.Email.Value, user.DisplayName, user.CreatedAt);
}
