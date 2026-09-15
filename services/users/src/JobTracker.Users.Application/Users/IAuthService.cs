using JobTracker.Users.Application.Users.Dtos;

namespace JobTracker.Users.Application.Users;

/// <summary>Authentication use cases: create an account and exchange credentials for a token.</summary>
public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
