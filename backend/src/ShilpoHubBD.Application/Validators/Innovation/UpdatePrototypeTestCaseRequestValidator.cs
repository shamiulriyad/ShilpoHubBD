using FluentValidation;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Validators.Innovation;

public class UpdatePrototypeTestCaseRequestValidator : AbstractValidator<UpdatePrototypeTestCaseRequest>
{
    public UpdatePrototypeTestCaseRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Steps).MaximumLength(6000);
        RuleFor(x => x.ExpectedResult).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Priority)
            .NotEmpty()
            .Must(p => Enum.TryParse<TestCasePriority>(p, true, out _))
            .WithMessage("Priority must be one of: Low, Medium, High, Critical.");
    }
}
