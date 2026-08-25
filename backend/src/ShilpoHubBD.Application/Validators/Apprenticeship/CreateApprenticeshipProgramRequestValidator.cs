using FluentValidation;
using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Domain.Entities.Apprenticeship;

namespace ShilpoHubBD.Application.Validators.Apprenticeship;

public class CreateApprenticeshipProgramRequestValidator : AbstractValidator<CreateApprenticeshipProgramRequest>
{
    public CreateApprenticeshipProgramRequestValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .Must(t => Enum.TryParse<ProgramType>(t, true, out _))
            .WithMessage("Type must be either 'Internship' or 'Apprenticeship'.");

        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.EligibilityRequirements).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.DurationWeeks).GreaterThan(0).When(x => x.DurationWeeks.HasValue);
        RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be on or after the start date.");
    }
}
