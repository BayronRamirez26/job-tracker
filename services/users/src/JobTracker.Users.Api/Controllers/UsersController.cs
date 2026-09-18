using System.Security.Claims;
using JobTracker.Users.Application.Users;
using JobTracker.Users.Application.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Users.Api.Controllers;

/// <summary>Authenticated user endpoints. The whole controller requires a valid JWT.</summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IProfileService _profileService;

    public UsersController(IUserService userService, IProfileService profileService)
    {
        _userService = userService;
        _profileService = profileService;
    }

    /// <summary>Returns the currently authenticated user, identified by the token's 'sub' claim.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetByIdAsync(userId, cancellationToken);
        return Ok(user);
    }

    /// <summary>Returns the current user's professional profile, or null if they have none yet.</summary>
    [HttpGet("me/profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto?>> GetProfile(CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _profileService.GetAsync(userId, cancellationToken);
        return Ok(profile);
    }

    /// <summary>Replaces the current user's professional profile (e.g. the one built from their CV).</summary>
    [HttpPut("me/profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        UserProfileDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var saved = await _profileService.SaveAsync(userId, request, cancellationToken);
        return Ok(saved);
    }

    // 'sub' is the standard subject claim; MapInboundClaims=false keeps that exact name.
    private bool TryGetUserId(out Guid userId) => Guid.TryParse(User.FindFirstValue("sub"), out userId);
}
