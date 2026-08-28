using FluentValidation;
using ShilpoHubBD.Application.DTOs.Passport;
using ShilpoHubBD.Domain.Entities.Passport;

namespace ShilpoHubBD.Application.Validators.Passport;

public class CreateBadgeRequestValidator : AbstractValidator<CreateBadgeRequest>
{
    public CreateBadgeRequestValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);

        RuleFor(x => x.DistrictId).NotEmpty().When(x => x.Type == BadgeType.District)
            .WithMessage("DistrictId is required for district badges.");
        RuleFor(x => x.FestivalName).NotEmpty().MaximumLength(100).When(x => x.Type == BadgeType.Festival)
            .WithMessage("FestivalName is required for festival badges.");
        RuleFor(x => x.RequiredPurchaseCount).NotEmpty().GreaterThan(0).When(x => x.Type == BadgeType.Purchase)
            .WithMessage("RequiredPurchaseCount is required for purchase badges.");
        RuleFor(x => x.RequiredCheckInCount).NotEmpty().GreaterThan(0).When(x => x.Type == BadgeType.CheckIn)
            .WithMessage("RequiredCheckInCount is required for check-in badges.");
    }
}
