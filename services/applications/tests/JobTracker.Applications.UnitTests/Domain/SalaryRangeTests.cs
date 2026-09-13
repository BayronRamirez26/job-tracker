using JobTracker.Applications.Domain.Exceptions;
using JobTracker.Applications.Domain.ValueObjects;

namespace JobTracker.Applications.UnitTests.Domain;

public class SalaryRangeTests
{
    [Fact]
    public void Create_normalizes_currency_to_uppercase()
    {
        var salary = SalaryRange.Create(50_000, 90_000, "usd");

        Assert.Equal("USD", salary.Currency);
        Assert.Equal(50_000, salary.Min);
        Assert.Equal(90_000, salary.Max);
    }

    [Fact]
    public void Create_allows_equal_min_and_max()
    {
        var salary = SalaryRange.Create(50_000, 50_000, "USD");
        Assert.Equal(50_000, salary.Min);
    }

    [Fact]
    public void Create_throws_when_min_negative()
        => Assert.Throws<DomainException>(() => SalaryRange.Create(-1, 10, "USD"));

    [Fact]
    public void Create_throws_when_max_less_than_min()
        => Assert.Throws<DomainException>(() => SalaryRange.Create(100, 10, "USD"));

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("DOLLAR")]
    public void Create_throws_when_currency_not_three_letters(string currency)
        => Assert.Throws<DomainException>(() => SalaryRange.Create(10, 20, currency));

    [Fact]
    public void Ranges_with_same_values_are_equal_by_value()
    {
        var a = SalaryRange.Create(10, 20, "USD");
        var b = SalaryRange.Create(10, 20, "usd"); // normalized to USD

        Assert.Equal(a, b);       // value equality (record)
        Assert.True(a == b);
    }
}
