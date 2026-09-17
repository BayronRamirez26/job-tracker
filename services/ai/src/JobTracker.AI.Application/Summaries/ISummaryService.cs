using JobTracker.AI.Application.Summaries.Dtos;

namespace JobTracker.AI.Application.Summaries;

/// <summary>Use case: turn a job description into a concise summary via the AI provider.</summary>
public interface ISummaryService
{
    Task<SummaryResponse> SummarizeAsync(SummarizeRequest request, CancellationToken cancellationToken = default);
}
