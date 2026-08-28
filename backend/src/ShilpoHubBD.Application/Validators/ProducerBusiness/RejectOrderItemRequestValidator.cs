using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;

namespace ShilpoHubBD.Application.Validators.ProducerBusiness;

public class RejectOrderItemRequestValidator : AbstractValidator<RejectOrderItemRequest>
{
    public RejectOrderItemRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
