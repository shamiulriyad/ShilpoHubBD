using FluentValidation;
using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Validators.Commerce;

public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
{
    public CheckoutRequestValidator()
    {
        RuleFor(x => x.RecipientName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RecipientPhone).NotEmpty().Matches(@"^01[3-9]\d{8}$")
            .WithMessage("RecipientPhone must be a valid Bangladeshi mobile number (e.g. 01712345678).");
        RuleFor(x => x.ShippingAddressLine).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ShippingDistrictId).NotEmpty();
        RuleFor(x => x.PaymentMethod).IsInEnum();
    }
}
