using FluentValidation;
using ShilpoHubBD.Application.DTOs.Quotations;

namespace ShilpoHubBD.Application.Validators.Quotations;

public class CreateQuotationRequestValidator : AbstractValidator<CreateQuotationRequest>
{
    public CreateQuotationRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Requirements).MaximumLength(2000);
        RuleFor(x => x.RequiredDeliveryDate).GreaterThan(DateTime.UtcNow.Date);

        RuleFor(x => x.ProducerIds).NotEmpty().WithMessage("At least one producer must be selected.");
        RuleFor(x => x.ProducerIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("ProducerIds must not contain duplicates.");

        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one item is required.");
        RuleForEach(x => x.Items).SetValidator(new QuotationRequestItemInputValidator());
    }
}
