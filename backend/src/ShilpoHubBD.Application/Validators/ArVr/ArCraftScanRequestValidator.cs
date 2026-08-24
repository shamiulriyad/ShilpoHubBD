using FluentValidation;
using ShilpoHubBD.Application.DTOs.ArVr;

namespace ShilpoHubBD.Application.Validators.ArVr;

public class ArCraftScanRequestValidator : AbstractValidator<ArCraftScanRequest>
{
    public ArCraftScanRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(200);
    }
}
