using FluentValidation;
using ShilpoHubBD.Application.DTOs.MentorMatching;

namespace ShilpoHubBD.Application.Validators.MentorMatching;

public class MentorMatchRequestValidator : AbstractValidator<MentorMatchRequest>
{
    public MentorMatchRequestValidator()
    {
        RuleFor(x => x.MinYearsOfExperience).GreaterThanOrEqualTo(0).When(x => x.MinYearsOfExperience.HasValue);
        RuleFor(x => x.MaxResults).InclusiveBetween(1, 50);
    }
}
