using FluentValidation;
using ShilpoHubBD.Application.DTOs.BusinessPartner;

namespace ShilpoHubBD.Application.Validators.BusinessPartner;

public class BusinessPartnerQueryParametersValidator : AbstractValidator<BusinessPartnerQueryParameters>
{
    public BusinessPartnerQueryParametersValidator()
    {
        RuleFor(x => x.Search).MaximumLength(200);
        RuleFor(x => x.BusinessType).IsInEnum().When(x => x.BusinessType.HasValue);
        RuleFor(x => x.VerificationStatus).IsInEnum().When(x => x.VerificationStatus.HasValue);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
