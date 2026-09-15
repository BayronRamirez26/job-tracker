namespace JobTracker.Users.Application.Abstractions;

/// <summary>
/// Port for password hashing. Declared here so the Application layer can hash and verify without
/// knowing the algorithm; Infrastructure supplies a PBKDF2 implementation. We never roll our own.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string password);
}
