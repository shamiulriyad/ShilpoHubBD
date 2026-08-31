using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreatePrototypeIssueRequestValidator : AbstractValidator<CreatePrototypeIssueRequest>
{
    public CreatePrototypeIssueRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(6000);
        RuleFor(x => x.Severity)
            .Must(s => string.IsNullOrWhiteSpace(s) || Enum.TryParse<PrototypeIssueSeverity>(s, true, out _))
            .WithMessage("Severity must be one of: Low, Medium, High, Critical.");
    }
}
