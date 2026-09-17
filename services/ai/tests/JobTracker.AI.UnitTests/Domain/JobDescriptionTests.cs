using JobTracker.AI.Domain.Exceptions;
using JobTracker.AI.Domain.ValueObjects;

namespace JobTracker.AI.UnitTests.Domain;

public class JobDescriptionTests
{
    [Fact]
    public void Create_trims_and_keeps_value()
    {
        var jd = JobDescription.Create("  Senior Engineer at Acme  ");
        Assert.Equal("Senior Engineer at Acme", jd.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_throws_when_empty(string value)
        => Assert.Throws<DomainException>(() => JobDescription.Create(value));

    [Fact]
    public void Create_throws_when_too_long()
        => Assert.Throws<DomainException>(() =>
            JobDescription.Create(new string('x', JobDescription.MaxLength + 1)));
}
