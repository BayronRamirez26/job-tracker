using JobTracker.Applications.Domain.Entities;
using JobTracker.Applications.Domain.Enums;
using JobTracker.Applications.Domain.Exceptions;
using JobTracker.Applications.Domain.ValueObjects;

namespace JobTracker.Applications.UnitTests.Domain;

public class JobApplicationTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Create_sets_fields_generates_id_and_timestamps()
    {
        var app = JobApplication.Create(UserId, "  Acme  ", "  Engineer  ", ApplicationStatus.Applied, ApplicationSource.LinkedIn);

        Assert.NotEqual(Guid.Empty, app.Id);
        Assert.Equal(UserId, app.UserId);
        Assert.Equal("Acme", app.Company);        // trimmed
        Assert.Equal("Engineer", app.Position);   // trimmed
        Assert.Equal(ApplicationStatus.Applied, app.Status);
        Assert.Equal(ApplicationSource.LinkedIn, app.Source);
        Assert.Equal(app.CreatedAt, app.UpdatedAt);
        Assert.Null(app.AppliedDate);
        Assert.Null(app.Salary);
    }

    [Fact]
    public void Create_throws_when_owner_is_empty()
        => Assert.Throws<DomainException>(() =>
            JobApplication.Create(Guid.Empty, "Acme", "Engineer", ApplicationStatus.Applied, ApplicationSource.Other));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_throws_when_company_blank(string company)
    {
        var ex = Assert.Throws<DomainException>(() =>
            JobApplication.Create(UserId, company, "Engineer", ApplicationStatus.Applied, ApplicationSource.Other));

        Assert.Contains("Company", ex.Message);
    }

    [Fact]
    public void Create_throws_when_position_blank()
        => Assert.Throws<DomainException>(() =>
            JobApplication.Create(UserId, "Acme", " ", ApplicationStatus.Applied, ApplicationSource.Other));

    [Fact]
    public void ChangeStatus_updates_status_and_bumps_updatedAt()
    {
        var app = JobApplication.Create(UserId, "Acme", "Engineer", ApplicationStatus.Applied, ApplicationSource.Other);
        var createdAt = app.CreatedAt;
        var updatedBefore = app.UpdatedAt;

        app.ChangeStatus(ApplicationStatus.Interview);

        Assert.Equal(ApplicationStatus.Interview, app.Status);
        Assert.Equal(createdAt, app.CreatedAt);            // never changes
        Assert.True(app.UpdatedAt >= updatedBefore);
    }

    [Fact]
    public void UpdateDetails_replaces_fields_and_preserves_createdAt()
    {
        var app = JobApplication.Create(UserId, "Acme", "Engineer", ApplicationStatus.Applied, ApplicationSource.Other);
        var createdAt = app.CreatedAt;
        var salary = SalaryRange.Create(50_000, 60_000, "usd");

        app.UpdateDetails("Globex", "Staff Engineer", ApplicationSource.Referral,
            new DateOnly(2026, 1, 1), "note", salary);

        Assert.Equal("Globex", app.Company);
        Assert.Equal("Staff Engineer", app.Position);
        Assert.Equal(ApplicationSource.Referral, app.Source);
        Assert.Equal(new DateOnly(2026, 1, 1), app.AppliedDate);
        Assert.Equal("note", app.Notes);
        Assert.Equal(salary, app.Salary);
        Assert.Equal(createdAt, app.CreatedAt);
    }
}
