using JobTracker.AI.Application.Extraction;
using JobTracker.AI.Application.Extraction.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.AI.Api.Controllers;

/// <summary>AI extraction endpoint: turn a job posting into structured application fields.</summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public sealed class ExtractionController : ControllerBase
{
    private readonly IExtractionService _extractionService;

    public ExtractionController(IExtractionService extractionService)
    {
        _extractionService = extractionService;
    }

    /// <summary>Extracts company, position, salary, and a notes summary from a job description.</summary>
    [HttpPost("extract")]
    [ProducesResponseType(typeof(ExtractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ExtractionResponse>> Extract(
        ExtractRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _extractionService.ExtractAsync(request, cancellationToken);
        return Ok(result);
    }
}
