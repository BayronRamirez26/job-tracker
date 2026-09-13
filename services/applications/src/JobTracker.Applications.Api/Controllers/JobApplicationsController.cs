using JobTracker.Applications.Application.JobApplications;
using JobTracker.Applications.Application.JobApplications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Applications.Api.Controllers;

/// <summary>
/// HTTP endpoints for managing job applications. The controller is intentionally thin: it maps
/// HTTP to use-case calls and back, and holds no business logic. Error-to-status mapping is
/// handled centrally by <see cref="Exceptions.GlobalExceptionHandler"/>, which is why these
/// actions never catch exceptions themselves.
/// </summary>
[ApiController]
[Route("api/job-applications")]
[Produces("application/json")]
public sealed class JobApplicationsController : ControllerBase
{
    private readonly IJobApplicationService _service;

    public JobApplicationsController(IJobApplicationService service)
    {
        _service = service;
    }

    /// <summary>Lists all job applications, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JobApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<JobApplicationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var results = await _service.GetAllAsync(cancellationToken);
        return Ok(results);
    }

    /// <summary>Gets a single job application by its id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates a new job application.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobApplicationResponse>> Create(
        CreateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(request, cancellationToken);

        // 201 Created with a Location header pointing at GetById.
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Replaces the editable fields of an existing job application.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationResponse>> Update(
        Guid id,
        UpdateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Deletes a job application.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
