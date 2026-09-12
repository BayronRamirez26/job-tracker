using FluentValidation;
using JobTracker.Applications.Application.JobApplications.Dtos;

namespace JobTracker.Applications.Application.JobApplications.Validators;

/// <summary>
/// Validates the shape of the create payload. Note the deliberate distinction from domain
/// invariants: this validator catches malformed <i>input</i> and returns a friendly 400 listing
/// every problem at once, while the Domain layer independently guarantees its rules can never be
/// violated regardless of caller. The two overlap on purpose — belt and braces.
/// </summary>
public sealed class CreateJobApplicationRequestValidator : AbstractValidator<CreateJobApplicationRequest>
{
    public CreateJobApplicationRequestValidator()
    {
        // NOTE: keep these max lengths in sync with the EF Core column configuration
        // we will add in the Infrastructure phase.
        RuleFor(x => x.Company)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Position)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Source).IsInEnum();

        RuleFor(x => x.Notes)
            .MaximumLength(2000);

        RuleFor(x => x.Salary!)
            .SetValidator(new SalaryRangeDtoValidator())
            .When(x => x.Salary is not null);
    }
}
