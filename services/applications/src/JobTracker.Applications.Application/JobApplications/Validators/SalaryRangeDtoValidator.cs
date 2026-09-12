using FluentValidation;
using JobTracker.Applications.Application.JobApplications.Dtos;

namespace JobTracker.Applications.Application.JobApplications.Validators;

/// <summary>Validates a <see cref="SalaryRangeDto"/>. Reused by both the create and update validators.</summary>
public sealed class SalaryRangeDtoValidator : AbstractValidator<SalaryRangeDto>
{
    public SalaryRangeDtoValidator()
    {
        RuleFor(x => x.Min)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Salary minimum cannot be negative.");

        RuleFor(x => x.Max)
            .GreaterThanOrEqualTo(x => x.Min)
            .WithMessage("Salary maximum must be greater than or equal to the minimum.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO 4217 code (e.g. USD).");
    }
}
