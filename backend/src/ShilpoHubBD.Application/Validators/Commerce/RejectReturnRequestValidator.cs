using FluentValidation;
using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Validators.Commerce;

public class RejectReturnRequestValidator : AbstractValidator<RejectReturnRequest>
{
    public RejectReturnRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
