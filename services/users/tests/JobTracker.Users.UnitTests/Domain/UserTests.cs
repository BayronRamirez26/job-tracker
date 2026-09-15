using JobTracker.Users.Domain.Entities;
using JobTracker.Users.Domain.Exceptions;
using JobTracker.Users.Domain.ValueObjects;

namespace JobTracker.Users.UnitTests.Domain;

public class UserTests
{
    private static readonly Email SampleEmail = Email.Create("ada@example.com");

    [Fact]
    public void Create_sets_fields_and_generates_id()
    {
        var user = User.Create(SampleEmail, "hashed", "  Ada  ");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(SampleEmail, user.Email);
        Assert.Equal("hashed", user.PasswordHash);
        Assert.Equal("Ada", user.DisplayName);   // trimmed
        Assert.NotEqual(default, user.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_throws_when_password_hash_blank(string hash)
        => Assert.Throws<DomainException>(() => User.Create(SampleEmail, hash, "Ada"));

    [Fact]
    public void Create_throws_when_display_name_blank()
        => Assert.Throws<DomainException>(() => User.Create(SampleEmail, "hashed", " "));
}
