using FluentValidation;
using JobTracker.Applications.Application.Abstractions;
using JobTracker.Applications.Application.Exceptions;
using JobTracker.Applications.Application.JobApplications.Dtos;
using JobTracker.Applications.Domain.Entities;

namespace JobTracker.Applications.Application.JobApplications;

/// <summary>
/// Orchestrates the job-application use cases: validate the input, drive the domain aggregate,
/// and persist through the repository and unit-of-work ports. It holds no business rules of its
/// own — those live in the Domain — it only coordinates the steps of each use case.
/// </summary>
public sealed class JobApplicationService : IJobApplicationService
{
    private readonly IJobApplicationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateJobApplicationRequest> _createValidator;
    private readonly IValidator<UpdateJobApplicationRequest> _updateValidator;

    public JobApplicationService(
        IJobApplicationRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateJobApplicationRequest> createValidator,
        IValidator<UpdateJobApplicationRequest> updateValidator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<JobApplicationResponse> CreateAsync(
        CreateJobApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var application = JobApplication.Create(
            request.Company,
            request.Position,
            request.Status,
            request.Source,
            request.AppliedDate,
            request.Notes,
            request.Salary.ToDomain());

        await _repository.AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return application.ToResponse();
    }

    public async Task<JobApplicationResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For<JobApplication>(id);

        return application.ToResponse();
    }

    public async Task<IReadOnlyList<JobApplicationResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var applications = await _repository.ListAsync(cancellationToken);

        return applications.Select(application => application.ToResponse()).ToList();
    }

    public async Task<JobApplicationResponse> UpdateAsync(
        Guid id,
        UpdateJobApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        await _updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var application = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For<JobApplication>(id);

        // Two intents, two methods: descriptive edits, then the lifecycle change.
        application.UpdateDetails(
            request.Company,
            request.Position,
            request.Source,
            request.AppliedDate,
            request.Notes,
            request.Salary.ToDomain());

        application.ChangeStatus(request.Status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return application.ToResponse();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.For<JobApplication>(id);

        _repository.Remove(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
