using JobTracker.Applications.Application.JobApplications.Dtos;

namespace JobTracker.Applications.Application.JobApplications;

/// <summary>The use cases for managing job applications. Controllers depend on this abstraction.</summary>
public interface IJobApplicationService
{
    Task<JobApplicationResponse> CreateAsync(CreateJobApplicationRequest request, CancellationToken cancellationToken = default);

    Task<JobApplicationResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobApplicationResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<JobApplicationResponse> UpdateAsync(Guid id, UpdateJobApplicationRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
