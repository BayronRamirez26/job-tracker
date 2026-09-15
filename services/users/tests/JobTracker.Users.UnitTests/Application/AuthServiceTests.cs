using FluentValidation;
using JobTracker.Users.Application.Abstractions;
using JobTracker.Users.Application.Exceptions;
using JobTracker.Users.Application.Users;
using JobTracker.Users.Application.Users.Dtos;
using JobTracker.Users.Application.Users.Validators;
using JobTracker.Users.Domain.Entities;
using JobTracker.Users.Domain.ValueObjects;
using NSubstitute;

namespace JobTracker.Users.UnitTests.Application;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenGenerator _tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        // Real validators; mocked ports.
        _sut = new AuthService(
            _users, _unitOfWork, _passwordHasher, _tokenGenerator,
            new RegisterRequestValidator(), new LoginRequestValidator());
    }

    private static RegisterRequest ValidRegister() => new("Ada@Example.com", "Sup3rSecret!", "Ada Lovelace");

    [Fact]
    public async Task RegisterAsync_hashes_persists_and_returns_normalized_user()
    {
        _users.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(false);
        _passwordHasher.Hash("Sup3rSecret!").Returns("HASHED");

        var result = await _sut.RegisterAsync(ValidRegister());

        Assert.Equal("ada@example.com", result.Email);   // Email value object normalized it
        Assert.Equal("Ada Lovelace", result.DisplayName);
        await _users.Received(1).AddAsync(
            Arg.Is<User>(u => u.PasswordHash == "HASHED" && u.Email.Value == "ada@example.com"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_throws_Conflict_when_email_exists()
    {
        _users.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() => _sut.RegisterAsync(ValidRegister()));
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_throws_Validation_when_password_too_short()
    {
        var invalid = ValidRegister() with { Password = "short" };
        await Assert.ThrowsAsync<ValidationException>(() => _sut.RegisterAsync(invalid));
    }

    [Fact]
    public async Task LoginAsync_returns_token_when_credentials_valid()
    {
        var user = User.Create(Email.Create("ada@example.com"), "HASHED", "Ada");
        _users.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("HASHED", "Sup3rSecret!").Returns(true);
        _tokenGenerator.Generate(Arg.Any<User>())
            .Returns(new AccessToken("jwt-token", DateTimeOffset.UtcNow.AddHours(1)));

        var result = await _sut.LoginAsync(new LoginRequest("ada@example.com", "Sup3rSecret!"));

        Assert.Equal("jwt-token", result.Token);
    }

    [Fact]
    public async Task LoginAsync_throws_InvalidCredentials_when_user_not_found()
    {
        _users.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _sut.LoginAsync(new LoginRequest("ghost@example.com", "whatever1")));
    }

    [Fact]
    public async Task LoginAsync_throws_InvalidCredentials_when_password_wrong()
    {
        var user = User.Create(Email.Create("ada@example.com"), "HASHED", "Ada");
        _users.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _sut.LoginAsync(new LoginRequest("ada@example.com", "wrongpass")));
    }
}
