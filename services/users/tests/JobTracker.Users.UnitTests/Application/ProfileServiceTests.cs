using FluentValidation;
using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Application.Exceptions;
using JobTracker.Users.Application.Users;
using JobTracker.Users.Application.Users.Dtos;
using JobTracker.Users.Application.Users.Validators;
using JobTracker.Users.Domain.Entities;
using NSubstitute;

namespace JobTracker.Users.UnitTests.Application;

public class ProfileServiceTests
{
    private readonly IProfileRepository _repository = Substitute.For<IProfileRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProfileService _sut;

    private static readonly Guid UserId = Guid.NewGuid();

    public ProfileServiceTests()
    {
        _sut = new ProfileService(_repository, _unitOfWork, new SaveProfileRequestValidator());
    }

    private static ProfileContentDto SampleContent() => new(
        "Ada Lovelace",
        "Software Engineer",
        "Builds systems.",
        "London",
        8,
        new[] { "C#", "Angular" },
        new[] { new ProfileExperienceDto("Acme", "Lead", "2020-2023", new[] { "Shipped it" }) },
        new[] { new ProfileEducationDto("MIT", "BSc", "2016") },
        new[] { new ProfileCertificationDto("AZ-204", "Microsoft", "2022") },
        new[] { "github.com/ada" });

    private static SaveProfileRequest SampleRequest(string name = "Full-Stack Developer") => new(name, SampleContent());

    [Fact]
    public async Task CreateAsync_persists_and_round_trips_name_and_content()
    {
        var created = await _sut.CreateAsync(UserId, SampleRequest());

        await _repository.Received(1).AddAsync(Arg.Is<Profile>(p => p.UserId == UserId), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

        Assert.Equal("Full-Stack Developer", created.Name);
        Assert.Equal("Ada Lovelace", created.Content.FullName);
        Assert.Equal("Angular", created.Content.Skills[1]);
        Assert.Equal("Acme", created.Content.Experience[0].Company);
        Assert.Equal("AZ-204", created.Content.Certifications[0].Name);
        Assert.Equal("Microsoft", created.Content.Certifications[0].Issuer);
    }

    [Fact]
    public async Task UpdateAsync_renames_and_replaces_content()
    {
        var existing = Profile.Create(UserId, "Old name", "{}");
        _repository.GetByIdAsync(UserId, existing.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var updated = await _sut.UpdateAsync(UserId, existing.Id, SampleRequest("Sales Engineer"));

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal("Sales Engineer", updated.Name);
        Assert.Equal("Ada Lovelace", updated.Content.FullName);
        Assert.Equal("Sales Engineer", existing.Name); // the aggregate was mutated
    }

    [Fact]
    public async Task GetAsync_throws_NotFound_when_missing_or_not_owned()
    {
        _repository.GetByIdAsync(UserId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Profile?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetAsync(UserId, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_removes_and_saves()
    {
        var existing = Profile.Create(UserId, "Doomed", "{}");
        _repository.GetByIdAsync(UserId, existing.Id, Arg.Any<CancellationToken>()).Returns(existing);

        await _sut.DeleteAsync(UserId, existing.Id);

        _repository.Received(1).Remove(existing);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_maps_profiles_to_summaries()
    {
        var a = Profile.Create(UserId, "A", "{}");
        var b = Profile.Create(UserId, "B", "{}");
        _repository.ListAsync(UserId, Arg.Any<CancellationToken>()).Returns(new[] { a, b });

        var summaries = await _sut.ListAsync(UserId);

        Assert.Equal(2, summaries.Count);
        Assert.Contains(summaries, s => s.Name == "A");
        Assert.Contains(summaries, s => s.Name == "B");
    }

    [Fact]
    public async Task CreateAsync_throws_when_name_is_blank()
        => await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(UserId, SampleRequest("  ")));
}
