using FluentValidation;
using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Validators.Community;

public class CreateVillageRequestValidator : AbstractValidator<CreateVillageRequest>
{
    public CreateVillageRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Craft).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.ImageUrl).MaximumLength(2000);
        RuleFor(x => x.DistrictId).NotEmpty();
    }
}
