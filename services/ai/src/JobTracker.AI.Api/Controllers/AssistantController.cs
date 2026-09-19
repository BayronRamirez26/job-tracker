using JobTracker.AI.Application.Assistant;
using JobTracker.AI.Application.Assistant.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.AI.Api.Controllers;

/// <summary>
/// The AI application assistant: given a job posting and the candidate's profile, it drafts a cover
/// letter, a tailored résumé, and a fit assessment.
/// </summary>
[ApiController]
[Route("api/ai")]
[Produces("application/json")]
public sealed class AssistantController : ControllerBase
{
    private readonly IAssistantService _assistant;

    public AssistantController(IAssistantService assistant)
    {
        _assistant = assistant;
    }

    /// <summary>Drafts a cover letter tailored to the posting and the candidate's profile.</summary>
    [HttpPost("cover-letter")]
    [ProducesResponseType(typeof(CoverLetterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CoverLetterResponse>> CoverLetter(AssistRequest request, CancellationToken cancellationToken)
        => Ok(await _assistant.CoverLetterAsync(request, cancellationToken));

    /// <summary>Generates a résumé (Markdown) tailored to the posting from the candidate's profile.</summary>
    [HttpPost("tailor-cv")]
    [ProducesResponseType(typeof(TailoredCvResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<TailoredCvResponse>> TailorCv(AssistRequest request, CancellationToken cancellationToken)
        => Ok(await _assistant.TailoredCvAsync(request, cancellationToken));

    /// <summary>Scores how well the candidate fits the posting, with strengths and gaps.</summary>
    [HttpPost("fit")]
    [ProducesResponseType(typeof(FitResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<FitResponse>> Fit(AssistRequest request, CancellationToken cancellationToken)
        => Ok(await _assistant.FitAsync(request, cancellationToken));
}
