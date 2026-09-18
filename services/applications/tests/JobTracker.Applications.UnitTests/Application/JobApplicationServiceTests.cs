using FluentValidation;
using JobTracker.Applications.Application.Abstractions;
using JobTracker.Applications.Application.Exceptions;
using JobTracker.Applications.Application.JobApplications;
using JobTracker.Applications.Application.JobApplications.Dtos;
using JobTracker.Applications.Application.JobApplications.Validators;
using JobTracker.Applications.Domain.Entities;
using JobTracker.Applications.Domain.Enums;
using NSubstitute;

namespace JobTracker.Applications.UnitTests.Application;

public class JobApplicationServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IJobApplicationRepository _repository = Substitute.For<IJobApplicationRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly JobApplicationService _sut;

    public JobApplicationServiceTests()
    {
        // Use the REAL validators — they're pure, and validation is part of the behavior under test.
        _sut = new JobApplicationService(
            _repository,
            _unitOfWork,
            new CreateJobApplicationRequestValidator(),
            new UpdateJobApplicationRequestValidator());
    }

    private static CreateJobApplicationRequest ValidCreate() =>
        new("Acme", "Engineer", ApplicationStatus.Applied, ApplicationSource.LinkedIn, null, null, null);

    [Fact]
    public async Task CreateAsync_persists_owned_application_and_returns_mapped_response()
    {
        var result = await _sut.CreateAsync(UserId, ValidCreate());

        Assert.Equal("Acme", result.Company);
        Assert.NotEqual(Guid.Empty, result.Id);
        await _repository.Received(1).AddAsync(
            Arg.Is<JobApplication>(a => a.Company == "Acme" && a.UserId == UserId),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_throws_and_does_not_persist_when_invalid()
    {
        var invalid = ValidCreate() with { Company = "" };

        await Assert.ThrowsAsync<ValidationException>(() => _sut.CreateAsync(UserId, invalid));
        await _repository.DidNotReceive().AddAsync(Arg.Any<JobApplication>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_throws_NotFound_when_missing()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((JobApplication?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(UserId, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_throws_NotFound_and_does_not_save_when_missing()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((JobApplication?)null);
        var request = new UpdateJobApplicationRequest(
            "Acme", "Engineer", ApplicationStatus.Offer, ApplicationSource.Other, null, null, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(UserId, Guid.NewGuid(), request));
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_mutates_and_saves_when_found()
    {
        var existing = JobApplication.Create(UserId, "Acme", "Engineer", ApplicationStatus.Applied, ApplicationSource.LinkedIn);
        _repository.GetByIdAsync(UserId, existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        var request = new UpdateJobApplicationRequest(
            "Globex", "Staff", ApplicationStatus.Interview, ApplicationSource.Referral, null, "note", null);

        var result = await _sut.UpdateAsync(UserId, existing.Id, request);

        Assert.Equal("Globex", result.Company);
        Assert.Equal(ApplicationStatus.Interview, result.Status);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_removes_and_saves_when_found()
    {
        var existing = JobApplication.Create(UserId, "Acme", "Engineer", ApplicationStatus.Applied, ApplicationSource.LinkedIn);
        _repository.GetByIdAsync(UserId, existing.Id, Arg.Any<CancellationToken>()).Returns(existing);

        await _sut.DeleteAsync(UserId, existing.Id);

        _repository.Received(1).Remove(existing);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
