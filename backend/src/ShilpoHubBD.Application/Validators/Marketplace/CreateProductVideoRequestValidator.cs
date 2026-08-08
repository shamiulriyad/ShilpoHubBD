using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class CreateProductVideoRequestValidator : AbstractValidator<CreateProductVideoRequest>
{
    public CreateProductVideoRequestValidator()
    {
        RuleFor(x => x.VideoUrl).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Title).MaximumLength(200);
    }
}
