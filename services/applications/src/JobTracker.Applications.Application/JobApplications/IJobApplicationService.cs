using JobTracker.Applications.Application.JobApplications.Dtos;

namespace JobTracker.Applications.Application.JobApplications;

/// <summary>
/// The use cases for managing job applications. Every call is scoped to a userId (the authenticated
/// owner), so users only ever see and change their own applications.
/// </summary>
public interface IJobApplicationService
{
    Task<JobApplicationResponse> CreateAsync(Guid userId, CreateJobApplicationRequest request, CancellationToken cancellationToken = default);

    Task<JobApplicationResponse> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobApplicationResponse>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<JobApplicationResponse> UpdateAsync(Guid userId, Guid id, UpdateJobApplicationRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
}
