using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIShopping;

namespace ShilpoHubBD.Application.Validators.AIShopping;

public class GiftRecommendationRequestValidator : AbstractValidator<GiftRecommendationRequest>
{
    public GiftRecommendationRequestValidator()
    {
        RuleFor(x => x.Occasion).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RecipientInterest).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Budget).GreaterThan(0).When(x => x.Budget.HasValue);
    }
}
