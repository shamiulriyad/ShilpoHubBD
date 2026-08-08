using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class MarkLessonProgressRequestValidator : AbstractValidator<MarkLessonProgressRequest>
{
    public MarkLessonProgressRequestValidator()
    {
        RuleFor(x => x.LessonId).NotEmpty();
    }
}
