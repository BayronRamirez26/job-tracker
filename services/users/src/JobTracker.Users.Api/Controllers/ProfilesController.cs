using System.Security.Claims;
using JobTracker.Users.Application.Users;
using JobTracker.Users.Application.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Users.Api.Controllers;

/// <summary>
/// CRUD for the current user's named professional profiles. Every action is scoped to the token's
/// 'sub' claim, so a user only ever sees and edits their own profiles.
/// </summary>
[ApiController]
[Route("api/users/me/profiles")]
[Produces("application/json")]
[Authorize]
public sealed class ProfilesController : ControllerBase
{
    private readonly IProfileService _profiles;

    public ProfilesController(IProfileService profiles)
    {
        _profiles = profiles;
    }

    /// <summary>Lists the current user's profiles (name + timestamp only), newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProfileSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<ProfileSummaryDto>>> List(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _profiles.ListAsync(userId, cancellationToken));
    }

    /// <summary>Returns one full profile, or 404 if it doesn't exist or belongs to another user.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProfileDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDetailDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _profiles.GetAsync(userId, id, cancellationToken));
    }

    /// <summary>Creates a new profile for the current user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProfileDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileDetailDto>> Create(SaveProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var created = await _profiles.CreateAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>Replaces a profile's name and content.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProfileDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDetailDto>> Update(Guid id, SaveProfileRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _profiles.UpdateAsync(userId, id, request, cancellationToken));
    }

    /// <summary>Deletes a profile.</summary>
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

        await _profiles.DeleteAsync(userId, id, cancellationToken);
        return NoContent();
    }

    // 'sub' is the standard subject claim; MapInboundClaims=false keeps that exact name.
    private bool TryGetUserId(out Guid userId) => Guid.TryParse(User.FindFirstValue("sub"), out userId);
}
