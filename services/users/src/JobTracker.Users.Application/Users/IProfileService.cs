using JobTracker.Users.Application.Users.Dtos;

namespace JobTracker.Users.Application.Users;

/// <summary>Manages the current user's named professional profiles.</summary>
public interface IProfileService
{
    Task<IReadOnlyList<ProfileSummaryDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ProfileDetailDto> GetAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<ProfileDetailDto> CreateAsync(Guid userId, SaveProfileRequest request, CancellationToken cancellationToken = default);

    Task<ProfileDetailDto> UpdateAsync(Guid userId, Guid id, SaveProfileRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
