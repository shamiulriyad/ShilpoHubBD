using FluentValidation;
using ShilpoHubBD.Application.DTOs.Portfolio;

namespace ShilpoHubBD.Application.Validators.Portfolio;

public class UpdatePortfolioRequestValidator : AbstractValidator<UpdatePortfolioRequest>
{
    public UpdatePortfolioRequestValidator()
    {
        RuleFor(x => x.Headline).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(4000);
    }
}
