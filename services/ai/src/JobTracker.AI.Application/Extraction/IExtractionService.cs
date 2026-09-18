using JobTracker.AI.Application.Extraction.Dtos;

namespace JobTracker.AI.Application.Extraction;

public interface IExtractionService
{
    Task<ExtractionResponse> ExtractAsync(ExtractRequest request, CancellationToken cancellationToken = default);
}
