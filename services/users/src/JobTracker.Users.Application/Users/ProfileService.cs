using System.Text.Json;
using FluentValidation;
using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Application.Exceptions;
using JobTracker.Users.Application.Users.Dtos;
using JobTracker.Users.Domain.Entities;

namespace JobTracker.Users.Application.Users;

/// <summary>
/// Manages a user's named professional profiles. The structured content DTO is (de)serialized here
/// and handed to the aggregate as opaque JSON, keeping the profile's shape out of the domain. Every
/// operation is scoped to the caller so one user can never touch another's profiles.
/// </summary>
public sealed class ProfileService : IProfileService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IProfileRepository _profiles;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<SaveProfileRequest> _validator;

    public ProfileService(IProfileRepository profiles, IUnitOfWork unitOfWork, IValidator<SaveProfileRequest> validator)
    {
        _profiles = profiles;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<IReadOnlyList<ProfileSummaryDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profiles = await _profiles.ListAsync(userId, cancellationToken);
        return profiles.Select(p => new ProfileSummaryDto(p.Id, p.Name, p.UpdatedAt)).ToList();
    }

    public async Task<ProfileDetailDto> GetAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _profiles.GetByIdAsync(userId, id, cancellationToken)
            ?? throw NotFoundException.For<Profile>(id);

        return ToDetail(profile);
    }

    public async Task<ProfileDetailDto> CreateAsync(Guid userId, SaveProfileRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var profile = Profile.Create(userId, request.Name, Serialize(request.Content));
        await _profiles.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDetail(profile);
    }

    public async Task<ProfileDetailDto> UpdateAsync(Guid userId, Guid id, SaveProfileRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var profile = await _profiles.GetByIdAsync(userId, id, cancellationToken)
            ?? throw NotFoundException.For<Profile>(id);

        profile.Rename(request.Name);
        profile.UpdateContent(Serialize(request.Content));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDetail(profile);
    }

    public async Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _profiles.GetByIdAsync(userId, id, cancellationToken)
            ?? throw NotFoundException.For<Profile>(id);

        _profiles.Remove(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string Serialize(ProfileContentDto content) => JsonSerializer.Serialize(Normalize(content), JsonOptions);

    private static ProfileDetailDto ToDetail(Profile profile)
        => new(profile.Id, profile.Name, profile.CreatedAt, profile.UpdatedAt, Normalize(Deserialize(profile.ContentJson)));

    private static ProfileContentDto Deserialize(string json)
        => JsonSerializer.Deserialize<ProfileContentDto>(json, JsonOptions) ?? Empty;

    // Guarantees the collections are never null — profiles built before certifications existed, or a
    // sparse client payload, would otherwise deserialize to null lists.
    private static ProfileContentDto Normalize(ProfileContentDto content) => content with
    {
        Skills = OrEmpty(content.Skills),
        Experience = OrEmpty(content.Experience),
        Education = OrEmpty(content.Education),
        Certifications = OrEmpty(content.Certifications),
        Links = OrEmpty(content.Links),
    };

    private static IReadOnlyList<T> OrEmpty<T>(IReadOnlyList<T>? list) => list ?? Array.Empty<T>();

    private static readonly ProfileContentDto Empty = new(
        null, null, null, null, null,
        Array.Empty<string>(),
        Array.Empty<ProfileExperienceDto>(),
        Array.Empty<ProfileEducationDto>(),
        Array.Empty<ProfileCertificationDto>(),
        Array.Empty<string>());
}
