using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class CreateLessonRequestValidator : AbstractValidator<CreateLessonRequest>
{
    public CreateLessonRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(8000);
        RuleFor(x => x.VideoUrl).MaximumLength(2000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
