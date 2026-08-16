using FluentValidation;
using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Validators.AITourism;

public class CulturalRecommendationRequestValidator : AbstractValidator<CulturalRecommendationRequest>
{
    public CulturalRecommendationRequestValidator()
    {
        RuleFor(x => x.MaxResults).InclusiveBetween(1, 50);
        RuleForEach(x => x.Interests).MaximumLength(100);
    }
}
