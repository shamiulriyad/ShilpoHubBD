using FluentValidation;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Validators.Governance;

public class CreateComplaintRequestValidator : AbstractValidator<CreateComplaintRequest>
{
    public CreateComplaintRequestValidator()
    {
        RuleFor(x => x.Category).NotEmpty().Must(BeEnum<ComplaintCategory>).WithMessage("Invalid Category.");
        RuleFor(x => x.Priority).NotEmpty().Must(BeEnum<ComplaintPriority>).WithMessage("Invalid Priority.");
        RuleFor(x => x.AgainstType).NotEmpty().Must(BeEnum<MonitoringSubjectType>).WithMessage("Invalid AgainstType.");
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ComplainantName).MaximumLength(160);
        RuleFor(x => x.ComplainantContact).MaximumLength(200);
        RuleFor(x => x.AgainstLabel).MaximumLength(200);
    }

    private static bool BeEnum<T>(string v) where T : struct, Enum => Enum.TryParse<T>(v, true, out _);
}

public class UpdateComplaintRequestValidator : AbstractValidator<UpdateComplaintRequest>
{
    public UpdateComplaintRequestValidator()
    {
        RuleFor(x => x.Category).Must(v => Enum.TryParse<ComplaintCategory>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Category)).WithMessage("Invalid Category.");
        RuleFor(x => x.Priority).Must(v => Enum.TryParse<ComplaintPriority>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Priority)).WithMessage("Invalid Priority.");
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.AgainstLabel).MaximumLength(200);
    }
}

public class AddComplaintUpdateRequestValidator : AbstractValidator<AddComplaintUpdateRequest>
{
    public AddComplaintUpdateRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.NewStatus).Must(v => Enum.TryParse<ComplaintStatus>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.NewStatus))
            .WithMessage("NewStatus must be one of: Submitted, Triaged, InProgress, Resolved, Rejected, Closed.");
    }
}

public class AssignComplaintRequestValidator : AbstractValidator<AssignComplaintRequest>
{
    public AssignComplaintRequestValidator()
    {
        RuleFor(x => x.AssigneeUserId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}

public class ResolveComplaintRequestValidator : AbstractValidator<ResolveComplaintRequest>
{
    public ResolveComplaintRequestValidator()
    {
        RuleFor(x => x.Resolution).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Outcome)
            .NotEmpty()
            .Must(v => v.Equals("Resolved", StringComparison.OrdinalIgnoreCase)
                || v.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Outcome must be Resolved or Rejected.");
    }
}

public class LinkComplaintFlagRequestValidator : AbstractValidator<LinkComplaintFlagRequest>
{
    public LinkComplaintFlagRequestValidator()
        => RuleFor(x => x.MonitoringFlagId).NotEmpty();
}
