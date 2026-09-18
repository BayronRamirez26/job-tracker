using System.Text.Json;
using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Application.Exceptions;
using JobTracker.Users.Application.Users.Dtos;
using JobTracker.Users.Domain.Entities;

namespace JobTracker.Users.Application.Users;

/// <summary>
/// Reads and writes the user's professional profile. The structured DTO is (de)serialized here and
/// handed to the aggregate as opaque JSON, keeping the profile's shape out of the domain.
/// </summary>
public sealed class ProfileService : IProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileService(IUserRepository users, IUnitOfWork unitOfWork)
    {
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw NotFoundException.For<User>(userId);

        return user.ProfileJson is null
            ? null
            : JsonSerializer.Deserialize<UserProfileDto>(user.ProfileJson, JsonOptions);
    }

    public async Task<UserProfileDto> SaveAsync(Guid userId, UserProfileDto profile, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw NotFoundException.For<User>(userId);

        user.SetProfile(JsonSerializer.Serialize(profile, JsonOptions));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return profile;
    }
}
