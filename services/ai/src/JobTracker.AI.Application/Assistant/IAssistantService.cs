using JobTracker.AI.Application.Assistant.Dtos;

namespace JobTracker.AI.Application.Assistant;

/// <summary>
/// The AI application assistant: from a job posting and the candidate's profile, it drafts a cover
/// letter, a tailored résumé, and a fit assessment.
/// </summary>
public interface IAssistantService
{
    Task<CoverLetterResponse> CoverLetterAsync(AssistRequest request, CancellationToken cancellationToken = default);

    Task<TailoredCvResponse> TailoredCvAsync(AssistRequest request, CancellationToken cancellationToken = default);

    Task<FitResponse> FitAsync(AssistRequest request, CancellationToken cancellationToken = default);
}
