using FluentValidation;
using ShilpoHubBD.Application.DTOs.Employment;

namespace ShilpoHubBD.Application.Validators.Employment;

public class JobMatchRequestValidator : AbstractValidator<JobMatchRequest>
{
    public JobMatchRequestValidator()
    {
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0).When(x => x.YearsOfExperience.HasValue);
        RuleFor(x => x.MaxResults).InclusiveBetween(1, 50);
    }
}
