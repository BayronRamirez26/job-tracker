using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Application.Exceptions;
using JobTracker.Users.Application.Users.Dtos;
using JobTracker.Users.Domain.Entities;

namespace JobTracker.Users.Application.Users;

/// <summary>Read-side use cases for users.</summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;

    public UserService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For<User>(id);

        return user.ToResponse();
    }
}
