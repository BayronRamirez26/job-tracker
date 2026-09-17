namespace JobTracker.AI.Application.Exceptions;

/// <summary>
/// The upstream AI provider could not be reached or returned an error. Modeled distinctly from a
/// 500 so the API can answer 503 Service Unavailable — the request was fine, a dependency failed.
/// </summary>
public sealed class AiUnavailableException : Exception
{
    public AiUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
