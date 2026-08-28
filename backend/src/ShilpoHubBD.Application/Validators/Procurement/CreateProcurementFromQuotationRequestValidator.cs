using FluentValidation;
using ShilpoHubBD.Application.DTOs.Procurement;

namespace ShilpoHubBD.Application.Validators.Procurement;

public class CreateProcurementFromQuotationRequestValidator : AbstractValidator<CreateProcurementFromQuotationRequest>
{
    public CreateProcurementFromQuotationRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Budget).GreaterThanOrEqualTo(0).When(x => x.Budget.HasValue);
        RuleFor(x => x.DeliveryDeadline).GreaterThan(DateTime.UtcNow.Date).When(x => x.DeliveryDeadline.HasValue);
    }
}
