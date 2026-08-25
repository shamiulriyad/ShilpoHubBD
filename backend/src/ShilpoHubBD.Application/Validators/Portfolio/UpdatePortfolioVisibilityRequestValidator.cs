using FluentValidation;
using ShilpoHubBD.Application.DTOs.Portfolio;
using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Application.Validators.Portfolio;

public class UpdatePortfolioVisibilityRequestValidator : AbstractValidator<UpdatePortfolioVisibilityRequest>
{
    public UpdatePortfolioVisibilityRequestValidator()
    {
        RuleFor(x => x.Visibility)
            .NotEmpty()
            .Must(v => Enum.TryParse<PortfolioVisibility>(v, true, out _))
            .WithMessage("Visibility must be either 'Private' or 'Public'.");
    }
}
