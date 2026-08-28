using FluentValidation;
using ShilpoHubBD.Application.DTOs.Procurement;

namespace ShilpoHubBD.Application.Validators.Procurement;

public class CreateProcurementRequestValidator : AbstractValidator<CreateProcurementRequest>
{
    public CreateProcurementRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.Budget).GreaterThanOrEqualTo(0).When(x => x.Budget.HasValue);
        RuleFor(x => x.DeliveryDeadline).GreaterThan(DateTime.UtcNow.Date);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).SetValidator(new ProcurementItemInputValidator());
    }
}
