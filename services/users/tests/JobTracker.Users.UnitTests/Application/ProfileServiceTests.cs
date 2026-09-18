using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Application.Users;
using JobTracker.Users.Application.Users.Dtos;
using JobTracker.Users.Domain.Entities;
using JobTracker.Users.Domain.ValueObjects;
using NSubstitute;

namespace JobTracker.Users.UnitTests.Application;

public class ProfileServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProfileService _sut;

    public ProfileServiceTests()
    {
        _sut = new ProfileService(_users, _unitOfWork);
    }

    private static User NewUser() => User.Create(Email.Create("ada@example.com"), "hash", "Ada");

    private static UserProfileDto SampleProfile() => new(
        "Ada Lovelace",
        "Software Engineer",
        "Builds systems.",
        "London",
        8,
        new[] { "C#", "Angular" },
        new[] { new ProfileExperienceDto("Acme", "Lead", "2020-2023", new[] { "Shipped it" }) },
        new[] { new ProfileEducationDto("MIT", "BSc", "2016") },
        new[] { "github.com/ada" });

    [Fact]
    public async Task SaveAsync_persists_and_GetAsync_round_trips_the_profile()
    {
        var user = NewUser();
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        await _sut.SaveAsync(user.Id, SampleProfile());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.NotNull(user.ProfileJson);

        var loaded = await _sut.GetAsync(user.Id);

        Assert.Equal("Ada Lovelace", loaded!.FullName);
        Assert.Equal(8, loaded.YearsOfExperience);
        Assert.Equal("Angular", loaded.Skills[1]);
        Assert.Equal("Acme", loaded.Experience[0].Company);
        Assert.Equal("Shipped it", loaded.Experience[0].Highlights[0]);
        Assert.Equal("MIT", loaded.Education[0].Institution);
    }

    [Fact]
    public async Task GetAsync_returns_null_when_the_user_has_no_profile()
    {
        var user = NewUser();
        _users.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);

        Assert.Null(await _sut.GetAsync(user.Id));
    }
}
