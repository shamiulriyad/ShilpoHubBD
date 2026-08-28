using FluentValidation;
using ShilpoHubBD.Application.DTOs.Portfolio;

namespace ShilpoHubBD.Application.Validators.Portfolio;

public class UpdatePortfolioProjectRequestValidator : AbstractValidator<UpdatePortfolioProjectRequest>
{
    public UpdatePortfolioProjectRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000);
        RuleFor(x => x.ProjectUrl).MaximumLength(1000);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}
