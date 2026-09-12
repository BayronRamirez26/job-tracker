namespace JobTracker.Applications.Application.Exceptions;

/// <summary>
/// Thrown by a use case when a requested resource does not exist. This is an <i>application</i>
/// concern — not a domain-rule violation — so it lives here rather than in the Domain. The API
/// layer maps it to an HTTP 404.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>Convenience factory, e.g. <c>NotFoundException.For&lt;JobApplication&gt;(id)</c>.</summary>
    public static NotFoundException For<TResource>(object key)
        => new($"{typeof(TResource).Name} with id '{key}' was not found.");
}
