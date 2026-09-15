using FluentValidation;
using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Application.Exceptions;
using JobTracker.Users.Application.Users.Dtos;
using JobTracker.Users.Domain.Entities;
using JobTracker.Users.Domain.Exceptions;
using JobTracker.Users.Domain.ValueObjects;

namespace JobTracker.Users.Application.Users;

/// <summary>
/// Coordinates registration and login. It owns the auth <i>workflow</i> (validate, check
/// uniqueness, hash, persist / verify, issue token) but delegates every specialised concern to a
/// port: hashing to <see cref="IPasswordHasher"/>, token issuance to <see cref="ITokenGenerator"/>.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IUserRepository users,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenGenerator tokenGenerator,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _users = users;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<UserResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await _registerValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = Email.Create(request.Email);

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException($"Email '{email.Value}' is already registered.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(email, passwordHash, request.DisplayName);

        await _users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.ToResponse();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        // A malformed email is just another failed login — keep the response uniform (401),
        // never a 422 that would hint the address was structurally wrong.
        Email email;
        try
        {
            email = Email.Create(request.Email);
        }
        catch (DomainException)
        {
            throw new InvalidCredentialsException();
        }

        var user = await _users.GetByEmailAsync(email, cancellationToken)
            ?? throw new InvalidCredentialsException();

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        var token = _tokenGenerator.Generate(user);
        return new LoginResponse(token.Token, token.ExpiresAt);
    }
}
