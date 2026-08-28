using FluentValidation;
using ShilpoHubBD.Application.DTOs.AIIntelligence;

namespace ShilpoHubBD.Application.Validators.AIIntelligence;

public class SupplierRankingRequestValidator : AbstractValidator<SupplierRankingRequest>
{
    public SupplierRankingRequestValidator()
    {
        RuleFor(x => x.MaxResults).InclusiveBetween(1, 50);
    }
}
