using FluentValidation;
using JobTracker.Users.Application.Users.Dtos;

namespace JobTracker.Users.Application.Users.Validators;

/// <summary>
/// Login only checks that both fields are present. We deliberately do NOT validate the email
/// format here — any credential problem should come back as a uniform 401, never a 400 that
/// hints the email was "malformed but recognised".
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
