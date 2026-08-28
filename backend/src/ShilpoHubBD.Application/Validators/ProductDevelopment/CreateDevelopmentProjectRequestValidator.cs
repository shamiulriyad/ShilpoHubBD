using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;

namespace ShilpoHubBD.Application.Validators.ProductDevelopment;

public class CreateDevelopmentProjectRequestValidator : AbstractValidator<CreateDevelopmentProjectRequest>
{
    public CreateDevelopmentProjectRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BusinessRequirements).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ProductSpecifications).NotEmpty().MaximumLength(4000);
    }
}
