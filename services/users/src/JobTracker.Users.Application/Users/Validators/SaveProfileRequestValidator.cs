using FluentValidation;
using JobTracker.Users.Application.Users.Dtos;

namespace JobTracker.Users.Application.Users.Validators;

/// <summary>
/// Validates a profile save. The name is the one field the service treats as first-class (it labels
/// the profile in the picker); the content is opaque and free-form, so it is only required to exist.
/// </summary>
public sealed class SaveProfileRequestValidator : AbstractValidator<SaveProfileRequest>
{
    public SaveProfileRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Content)
            .NotNull();
    }
}
