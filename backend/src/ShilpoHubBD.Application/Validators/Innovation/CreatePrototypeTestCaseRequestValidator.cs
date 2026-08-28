using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class CreatePrototypeTestCaseRequestValidator : AbstractValidator<CreatePrototypeTestCaseRequest>
{
    public CreatePrototypeTestCaseRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Steps).MaximumLength(6000);
        RuleFor(x => x.ExpectedResult).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Priority)
            .Must(p => string.IsNullOrWhiteSpace(p) || Enum.TryParse<TestCasePriority>(p, true, out _))
            .WithMessage("Priority must be one of: Low, Medium, High, Critical.");
    }
}
