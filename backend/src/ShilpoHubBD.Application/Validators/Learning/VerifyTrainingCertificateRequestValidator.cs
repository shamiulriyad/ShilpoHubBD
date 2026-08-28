using FluentValidation;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Validators.Learning;

public class VerifyTrainingCertificateRequestValidator : AbstractValidator<VerifyTrainingCertificateRequest>
{
    public VerifyTrainingCertificateRequestValidator()
    {
        RuleFor(x => x.CertificateNumber).NotEmpty().MaximumLength(50);
    }
}
