using FluentValidation;
using ShilpoHubBD.Application.DTOs.QRVerification;

namespace ShilpoHubBD.Application.Validators.QRVerification;

public class GenerateQRCodeRequestValidator : AbstractValidator<GenerateQRCodeRequest>
{
    public GenerateQRCodeRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
