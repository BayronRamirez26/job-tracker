using FluentValidation;
using JobTracker.AI.Application.Summaries.Dtos;
using JobTracker.AI.Domain.ValueObjects;

namespace JobTracker.AI.Application.Summaries.Validators;

public sealed class SummarizeRequestValidator : AbstractValidator<SummarizeRequest>
{
    public SummarizeRequestValidator()
    {
        RuleFor(x => x.JobDescription)
            .NotEmpty()
            .MaximumLength(JobDescription.MaxLength);
    }
}
