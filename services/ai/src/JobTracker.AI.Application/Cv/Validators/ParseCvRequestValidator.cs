using FluentValidation;
using JobTracker.AI.Application.Cv.Dtos;

namespace JobTracker.AI.Application.Cv.Validators;

public sealed class ParseCvRequestValidator : AbstractValidator<ParseCvRequest>
{
    public const int MaxLength = 60_000;

    public ParseCvRequestValidator()
    {
        RuleFor(x => x.Cv)
            .NotEmpty()
            .MaximumLength(MaxLength);
    }
}
