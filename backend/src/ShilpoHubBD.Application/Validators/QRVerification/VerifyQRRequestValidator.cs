using FluentValidation;
using ShilpoHubBD.Application.DTOs.QRVerification;

namespace ShilpoHubBD.Application.Validators.QRVerification;

public class VerifyQRRequestValidator : AbstractValidator<VerifyQRRequest>
{
    public VerifyQRRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(100);
    }
}
