namespace JobTracker.Users.Application.Exceptions;

/// <summary>
/// Login failed. Deliberately generic ("invalid email or password") and used for both an unknown
/// email and a wrong password, so the API never reveals which accounts exist. Maps to HTTP 401.
/// </summary>
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid email or password.")
    {
    }
}
