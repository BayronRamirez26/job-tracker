namespace JobTracker.AI.Domain.Exceptions;

/// <summary>Thrown when a domain invariant would be violated. Mapped to HTTP 422 by the API.</summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
