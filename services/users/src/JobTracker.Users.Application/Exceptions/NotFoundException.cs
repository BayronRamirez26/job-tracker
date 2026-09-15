namespace JobTracker.Users.Application.Exceptions;

/// <summary>A requested resource does not exist. Maps to HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }

    public static NotFoundException For<TResource>(object key)
        => new($"{typeof(TResource).Name} with id '{key}' was not found.");
}
