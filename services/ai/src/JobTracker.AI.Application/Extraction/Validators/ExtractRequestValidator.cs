using FluentValidation;
using JobTracker.AI.Application.Extraction.Dtos;
using JobTracker.AI.Domain.ValueObjects;

namespace JobTracker.AI.Application.Extraction.Validators;

public sealed class ExtractRequestValidator : AbstractValidator<ExtractRequest>
{
    public ExtractRequestValidator()
    {
        RuleFor(x => x.JobDescription)
            .NotEmpty()
            .MaximumLength(JobDescription.MaxLength);
    }
}
