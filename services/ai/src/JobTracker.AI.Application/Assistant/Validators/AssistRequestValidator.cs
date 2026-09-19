using FluentValidation;
using JobTracker.AI.Application.Assistant.Dtos;
using JobTracker.AI.Domain.ValueObjects;

namespace JobTracker.AI.Application.Assistant.Validators;

public sealed class AssistRequestValidator : AbstractValidator<AssistRequest>
{
    public AssistRequestValidator()
    {
        RuleFor(x => x.JobDescription)
            .NotEmpty()
            .MaximumLength(JobDescription.MaxLength);
    }
}
