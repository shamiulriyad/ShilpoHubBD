using FluentValidation;
using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Validators.Reviews;

public class ReviewQueryParametersValidator : AbstractValidator<ReviewQueryParameters>
{
    public ReviewQueryParametersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
