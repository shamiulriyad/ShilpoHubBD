using FluentValidation;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Validators.Governance;

public class RunMonitoringScanRequestValidator : AbstractValidator<RunMonitoringScanRequest>
{
    private static readonly string[] ScanTypes = { "All", "Fraud", "FakeProduct", "ReviewAbuse", "QrAnomaly" };

    public RunMonitoringScanRequestValidator()
    {
        RuleFor(x => x.ScanType)
            .NotEmpty()
            .Must(v => ScanTypes.Contains(v, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"ScanType must be one of: {string.Join(", ", ScanTypes)}.");

        RuleFor(x => x.MinRiskScore).InclusiveBetween(0, 100).When(x => x.MinRiskScore.HasValue);
    }
}

public class CreateMonitoringFlagRequestValidator : AbstractValidator<CreateMonitoringFlagRequest>
{
    public CreateMonitoringFlagRequestValidator()
    {
        RuleFor(x => x.FlagType).NotEmpty().Must(BeEnum<MonitoringFlagType>).WithMessage("Invalid FlagType.");
        RuleFor(x => x.Severity).NotEmpty().Must(BeEnum<MonitoringFlagSeverity>).WithMessage("Invalid Severity.");
        RuleFor(x => x.SubjectType).NotEmpty().Must(BeEnum<MonitoringSubjectType>).WithMessage("Invalid SubjectType.");
        RuleFor(x => x.SubjectLabel).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RiskScore).InclusiveBetween(0, 100).When(x => x.RiskScore.HasValue);
    }

    private static bool BeEnum<T>(string v) where T : struct, Enum => Enum.TryParse<T>(v, true, out _);
}

public class UpdateMonitoringFlagStatusRequestValidator : AbstractValidator<UpdateMonitoringFlagStatusRequest>
{
    public UpdateMonitoringFlagStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(v => Enum.TryParse<MonitoringFlagStatus>(v, true, out _))
            .WithMessage("Status must be one of: Open, UnderReview, Confirmed, Dismissed, Resolved.");
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class AssignMonitoringFlagRequestValidator : AbstractValidator<AssignMonitoringFlagRequest>
{
    public AssignMonitoringFlagRequestValidator()
    {
        RuleFor(x => x.AssigneeUserId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class AddMonitoringFlagNoteRequestValidator : AbstractValidator<AddMonitoringFlagNoteRequest>
{
    public AddMonitoringFlagNoteRequestValidator()
        => RuleFor(x => x.Note).NotEmpty().MaximumLength(2000);
}
