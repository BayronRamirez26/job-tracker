namespace JobTracker.Applications.Domain.Exceptions;

/// <summary>
/// Thrown when a domain invariant would be violated — for example a blank company name or
/// an inverted salary range.
/// </summary>
/// <remarks>
/// Business rules live in the Domain layer, so it is the Domain layer that signals when one
/// is broken. Expressing that as a dedicated exception type (rather than a generic
/// <see cref="System.ArgumentException"/>) lets outer layers recognise "this was a business
/// rule violation" specifically: the API's exception-handling middleware will later map this
/// to an HTTP 400/422 response, while unexpected exceptions map to a 500.
/// </remarks>
public class DomainException : Exception
{
    public DomainException(string message)
        : base(message)
    {
    }
}
