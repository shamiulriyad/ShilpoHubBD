using FluentValidation;
using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.Validators.Marketplace;

public class UpdateDistrictRequestValidator : AbstractValidator<UpdateDistrictRequest>
{
    public UpdateDistrictRequestValidator()
    {
        RuleFor(x => x.Division).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
