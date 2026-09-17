using FluentValidation;
using ShilpoHubBD.Application.DTOs.Cms;

namespace ShilpoHubBD.Application.Validators.Cms;

public class UpdateBlogPostRequestValidator : AbstractValidator<UpdateBlogPostRequest>
{
    public UpdateBlogPostRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(20000);
        RuleFor(x => x.CoverImageUrl).MaximumLength(1000);
        RuleFor(x => x.Tags).MaximumLength(500);
    }
}
