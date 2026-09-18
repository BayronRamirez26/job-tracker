using JobTracker.AI.Application.Cv.Dtos;

namespace JobTracker.AI.Application.Cv;

public interface ICvService
{
    Task<CvProfileResponse> ParseAsync(ParseCvRequest request, CancellationToken cancellationToken = default);
}
