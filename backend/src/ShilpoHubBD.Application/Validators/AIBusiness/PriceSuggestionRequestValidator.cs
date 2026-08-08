using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIBusiness;

namespace ShilpoHubBD.Application.Validators.AIBusiness;

public class PriceSuggestionRequestValidator : AbstractValidator<PriceSuggestionRequest>
{
    public PriceSuggestionRequestValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.EstimatedCost).GreaterThan(0).When(x => x.EstimatedCost.HasValue);
        RuleFor(x => x.DesiredMarginPercent).GreaterThan(0).When(x => x.DesiredMarginPercent.HasValue);
    }
}
