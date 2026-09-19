using FluentValidation;
using JobTracker.Applications.Application.JobApplications.Dtos;

namespace JobTracker.Applications.Application.JobApplications.Validators;

/// <summary>Validates the shape of the update payload (same rules as create).</summary>
public sealed class UpdateJobApplicationRequestValidator : AbstractValidator<UpdateJobApplicationRequest>
{
    public UpdateJobApplicationRequestValidator()
    {
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

        RuleFor(x => x.JobDescription)
            .MaximumLength(20000);

        RuleFor(x => x.Salary!)
            .SetValidator(new SalaryRangeDtoValidator())
            .When(x => x.Salary is not null);
    }
}
