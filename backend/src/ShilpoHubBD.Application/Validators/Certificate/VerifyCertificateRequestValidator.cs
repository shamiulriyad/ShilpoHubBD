using FluentValidation;
using ShilpoHubBD.Application.DTOs.Certificate;

namespace ShilpoHubBD.Application.Validators.Certificate;

public class VerifyCertificateRequestValidator : AbstractValidator<VerifyCertificateRequest>
{
    public VerifyCertificateRequestValidator()
    {
        RuleFor(x => x.CertificateNumber).NotEmpty().MaximumLength(50);
    }
}
