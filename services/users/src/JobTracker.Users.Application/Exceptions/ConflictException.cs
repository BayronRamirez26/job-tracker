namespace JobTracker.Users.Application.Exceptions;

/// <summary>A request conflicts with existing state (e.g. an email already registered). Maps to HTTP 409.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
