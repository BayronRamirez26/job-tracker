using FluentValidation;
using JobTracker.Users.Application.Users.Dtos;

namespace JobTracker.Users.Application.Users.Validators;

/// <summary>
/// Validates the register payload. Password-strength rules live here (not in the domain) because
/// they concern the plaintext <i>input</i>, which the domain never sees.
/// </summary>
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(100);
    }
}
