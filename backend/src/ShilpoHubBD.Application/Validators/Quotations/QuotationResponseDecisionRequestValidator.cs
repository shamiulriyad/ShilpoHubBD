using FluentValidation;
using ShilpoHubBD.Application.DTOs.Quotations;
using ShilpoHubBD.Domain.Entities.Quotations;

namespace ShilpoHubBD.Application.Validators.Quotations;

public class QuotationResponseDecisionRequestValidator : AbstractValidator<QuotationResponseDecisionRequest>
{
    public QuotationResponseDecisionRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(s => s is QuotationResponseStatus.Accepted or QuotationResponseStatus.Rejected)
            .WithMessage("Status must be Accepted or Rejected.");
        RuleFor(x => x.DecisionNotes).MaximumLength(1000);
    }
}
