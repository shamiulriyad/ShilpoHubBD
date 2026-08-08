using FluentValidation;
using ShilpoHubBD.Application.DTOs.Certificate;

namespace ShilpoHubBD.Application.Validators.Certificate;

public class GenerateCertificateRequestValidator : AbstractValidator<GenerateCertificateRequest>
{
    public GenerateCertificateRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
    }
}
