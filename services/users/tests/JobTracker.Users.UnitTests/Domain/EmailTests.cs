using JobTracker.Users.Domain.Exceptions;
using JobTracker.Users.Domain.ValueObjects;

namespace JobTracker.Users.UnitTests.Domain;

public class EmailTests
{
    [Fact]
    public void Create_trims_and_lowercases()
    {
        var email = Email.Create("  Ada@Example.COM  ");
        Assert.Equal("ada@example.com", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-at-sign")]
    [InlineData("@example.com")]      // empty local part
    [InlineData("ada@")]              // nothing after @
    [InlineData("ada@localhost")]     // no dot in domain
    [InlineData("ada@@example.com")]  // two @
    public void Create_throws_for_invalid(string value)
        => Assert.Throws<DomainException>(() => Email.Create(value));

    [Fact]
    public void Equal_by_value_after_normalization()
        => Assert.Equal(Email.Create("ADA@example.com"), Email.Create("ada@example.com"));
}
