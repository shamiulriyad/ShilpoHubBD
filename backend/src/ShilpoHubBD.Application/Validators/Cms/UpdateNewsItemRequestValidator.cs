using FluentValidation;
using ShilpoHubBD.Application.DTOs.Cms;

namespace ShilpoHubBD.Application.Validators.Cms;

public class UpdateNewsItemRequestValidator : AbstractValidator<UpdateNewsItemRequest>
{
    public UpdateNewsItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.Source).MaximumLength(200);
    }
}
