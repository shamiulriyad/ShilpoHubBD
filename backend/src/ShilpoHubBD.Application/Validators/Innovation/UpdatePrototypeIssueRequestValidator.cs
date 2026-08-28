using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class UpdatePrototypeIssueRequestValidator : AbstractValidator<UpdatePrototypeIssueRequest>
{
    public UpdatePrototypeIssueRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.Resolution).MaximumLength(4000);
        RuleFor(x => x.Severity)
            .NotEmpty()
            .Must(s => Enum.TryParse<PrototypeIssueSeverity>(s, true, out _))
            .WithMessage("Severity must be one of: Low, Medium, High, Critical.");
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<PrototypeIssueStatus>(s, true, out _))
            .WithMessage("Status must be one of: Open, InProgress, Resolved, WontFix, Closed.");
    }
}
