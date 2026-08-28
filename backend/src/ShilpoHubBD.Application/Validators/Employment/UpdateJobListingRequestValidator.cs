using FluentValidation;
using ShilpoHubBD.Application.DTOs.Employment;

namespace ShilpoHubBD.Application.Validators.Employment;

public class UpdateJobListingRequestValidator : AbstractValidator<UpdateJobListingRequest>
{
    public UpdateJobListingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.MinExperienceYears).GreaterThanOrEqualTo(0).When(x => x.MinExperienceYears.HasValue);
        RuleFor(x => x.SalaryMin).GreaterThanOrEqualTo(0).When(x => x.SalaryMin.HasValue);
        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(x => x.SalaryMin!.Value)
            .When(x => x.SalaryMin.HasValue && x.SalaryMax.HasValue)
            .WithMessage("Maximum salary must be greater than or equal to the minimum salary.");
    }
}
