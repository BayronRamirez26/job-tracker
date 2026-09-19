using JobTracker.Users.Domain.Entities;
using JobTracker.Users.Domain.Exceptions;

namespace JobTracker.Users.UnitTests.Domain;

public class ProfileTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_sets_owner_name_and_timestamps()
    {
        var profile = Profile.Create(UserId, "  Full-Stack Developer  ", "{\"headline\":\"Engineer\"}");

        Assert.NotEqual(Guid.Empty, profile.Id);
        Assert.Equal(UserId, profile.UserId);
        Assert.Equal("Full-Stack Developer", profile.Name); // trimmed
        Assert.Equal(profile.CreatedAt, profile.UpdatedAt);
    }

    [Fact]
    public void Create_throws_when_owner_is_empty()
        => Assert.Throws<DomainException>(() => Profile.Create(Guid.Empty, "Dev", "{}"));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_throws_when_name_is_blank(string name)
        => Assert.Throws<DomainException>(() => Profile.Create(UserId, name, "{}"));

    [Fact]
    public void Create_defaults_empty_content_to_an_empty_document()
    {
        var profile = Profile.Create(UserId, "Dev", null);

        Assert.Equal("{}", profile.ContentJson);
    }

    [Fact]
    public void Rename_updates_name_and_bumps_updatedAt()
    {
        var profile = Profile.Create(UserId, "Dev", "{}");
        var createdAt = profile.CreatedAt;

        profile.Rename("  Sales Engineer  ");

        Assert.Equal("Sales Engineer", profile.Name);
        Assert.Equal(createdAt, profile.CreatedAt); // never changes
        Assert.True(profile.UpdatedAt >= createdAt);
    }
}
