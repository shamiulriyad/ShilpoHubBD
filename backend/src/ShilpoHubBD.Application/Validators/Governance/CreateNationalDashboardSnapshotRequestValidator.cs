using FluentValidation;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Validators.Governance;

public class CreateNationalDashboardSnapshotRequestValidator
    : AbstractValidator<CreateNationalDashboardSnapshotRequest>
{
    public CreateNationalDashboardSnapshotRequestValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(120);

        RuleFor(x => x.Period)
            .NotEmpty()
            .Must(v => Enum.TryParse<DashboardPeriod>(v, true, out _))
            .WithMessage("Period must be one of: Monthly, Quarterly, Yearly, Custom.");

        RuleFor(x => x.PeriodStart).NotEmpty();
        RuleFor(x => x.PeriodEnd)
            .NotEmpty()
            .GreaterThan(x => x.PeriodStart)
            .WithMessage("PeriodEnd must be after PeriodStart.");

        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
