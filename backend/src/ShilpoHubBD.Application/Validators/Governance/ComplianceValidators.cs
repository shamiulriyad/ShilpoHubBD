using FluentValidation;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Validators.Governance;

public class CreateComplianceRecordRequestValidator : AbstractValidator<CreateComplianceRecordRequest>
{
    public CreateComplianceRecordRequestValidator()
    {
        RuleFor(x => x.EntityType).NotEmpty().Must(BeEnum<ComplianceEntityType>).WithMessage("Invalid EntityType.");
        RuleFor(x => x.EntityLabel).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Framework).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleForEach(x => x.Requirements).SetValidator(new UpsertComplianceRequirementRequestValidator());
    }

    private static bool BeEnum<T>(string v) where T : struct, Enum => Enum.TryParse<T>(v, true, out _);
}

public class UpdateComplianceRecordRequestValidator : AbstractValidator<UpdateComplianceRecordRequest>
{
    public UpdateComplianceRecordRequestValidator()
    {
        RuleFor(x => x.Framework).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(4000);
        RuleFor(x => x.Status).Must(v => Enum.TryParse<ComplianceStatus>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be one of: NotStarted, InProgress, Compliant, NonCompliant, Waived, Expired.");
    }
}

public class UpsertComplianceRequirementRequestValidator : AbstractValidator<UpsertComplianceRequirementRequest>
{
    public UpsertComplianceRequirementRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Evidence).MaximumLength(1000);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(v => Enum.TryParse<ComplianceRequirementStatus>(v, true, out _))
            .WithMessage("Status must be one of: Met, Unmet, Partial, NotApplicable.");
    }
}
