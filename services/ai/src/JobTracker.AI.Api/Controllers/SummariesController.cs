using JobTracker.AI.Application.Summaries;
using JobTracker.AI.Application.Summaries.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.AI.Api.Controllers;

/// <summary>AI endpoints for job applications.</summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public sealed class SummariesController : ControllerBase
{
    private readonly ISummaryService _summaryService;

    public SummariesController(ISummaryService summaryService)
    {
        _summaryService = summaryService;
    }

    /// <summary>Summarizes a job description with Claude.</summary>
    [HttpPost("summarize")]
    [ProducesResponseType(typeof(SummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<SummaryResponse>> Summarize(SummarizeRequest request, CancellationToken cancellationToken)
    {
        var result = await _summaryService.SummarizeAsync(request, cancellationToken);
        return Ok(result);
    }
}
