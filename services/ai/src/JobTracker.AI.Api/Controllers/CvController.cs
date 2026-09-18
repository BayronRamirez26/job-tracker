using JobTracker.AI.Application.Cv;
using JobTracker.AI.Application.Cv.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.AI.Api.Controllers;

/// <summary>AI CV endpoint: turn a CV into a structured professional profile.</summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public sealed class CvController : ControllerBase
{
    private readonly ICvService _cvService;

    public CvController(ICvService cvService)
    {
        _cvService = cvService;
    }

    /// <summary>Builds a professional profile from a CV (plain text or LaTeX source).</summary>
    [HttpPost("parse-cv")]
    [ProducesResponseType(typeof(CvProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CvProfileResponse>> ParseCv(
        ParseCvRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cvService.ParseAsync(request, cancellationToken);
        return Ok(result);
    }
}
