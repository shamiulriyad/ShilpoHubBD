using FluentValidation;
using ShilpoHubBD.Application.DTOs.Procurement;

namespace ShilpoHubBD.Application.Validators.Procurement;

public class ProcurementDecisionRequestValidator : AbstractValidator<ProcurementDecisionRequest>
{
    public ProcurementDecisionRequestValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
