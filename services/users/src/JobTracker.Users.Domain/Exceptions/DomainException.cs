namespace JobTracker.Users.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant would be violated. Duplicated per-service (rather than shared)
/// to keep each service independently deployable; the API maps it to an HTTP 422.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
