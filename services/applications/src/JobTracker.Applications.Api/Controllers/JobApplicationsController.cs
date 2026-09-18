using System.Security.Claims;
using JobTracker.Applications.Application.JobApplications;
using JobTracker.Applications.Application.JobApplications.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Applications.Api.Controllers;

/// <summary>
/// HTTP endpoints for managing job applications. The whole controller requires a valid JWT, and
/// every action is scoped to the caller (the token's 'sub' claim), so users only ever touch their
/// own applications. Error-to-status mapping is handled by the GlobalExceptionHandler.
/// </summary>
[ApiController]
[Route("api/job-applications")]
[Produces("application/json")]
[Authorize]
public sealed class JobApplicationsController : ControllerBase
{
    private readonly IJobApplicationService _service;

    public JobApplicationsController(IJobApplicationService service)
    {
        _service = service;
    }

    /// <summary>Lists the caller's job applications, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JobApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<JobApplicationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var results = await _service.GetAllAsync(userId, cancellationToken);
        return Ok(results);
    }

    /// <summary>Gets one of the caller's job applications by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await _service.GetByIdAsync(userId, id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Creates a new job application owned by the caller.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JobApplicationResponse>> Create(
        CreateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _service.CreateAsync(userId, request, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Replaces the editable fields of one of the caller's job applications.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JobApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobApplicationResponse>> Update(
        Guid id,
        UpdateJobApplicationRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var updated = await _service.UpdateAsync(userId, id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>Deletes one of the caller's job applications.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        await _service.DeleteAsync(userId, id, cancellationToken);
        return NoContent();
    }

    // 'sub' is the standard subject claim; MapInboundClaims=false keeps that exact name.
    private bool TryGetUserId(out Guid userId) => Guid.TryParse(User.FindFirstValue("sub"), out userId);
}
