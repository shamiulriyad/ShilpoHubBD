using FluentValidation;
using ShilpoHubBD.Application.DTOs.Employment;
using ShilpoHubBD.Domain.Entities.Employment;

namespace ShilpoHubBD.Application.Validators.Employment;

public class CreateJobListingRequestValidator : AbstractValidator<CreateJobListingRequest>
{
    public CreateJobListingRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.EmploymentType)
            .NotEmpty()
            .Must(t => Enum.TryParse<EmploymentType>(t, true, out _))
            .WithMessage("EmploymentType must be one of: FullTime, PartTime, Contract, Freelance, Internship.");
        RuleFor(x => x.MinExperienceYears).GreaterThanOrEqualTo(0).When(x => x.MinExperienceYears.HasValue);
        RuleFor(x => x.SalaryMin).GreaterThanOrEqualTo(0).When(x => x.SalaryMin.HasValue);
        RuleFor(x => x.SalaryMax)
            .GreaterThanOrEqualTo(x => x.SalaryMin!.Value)
            .When(x => x.SalaryMin.HasValue && x.SalaryMax.HasValue)
            .WithMessage("Maximum salary must be greater than or equal to the minimum salary.");
    }
}
