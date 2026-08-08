using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class UpdateCourseRequestValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.MaxApprentices).GreaterThan(0).When(x => x.MaxApprentices.HasValue);
    }
}
