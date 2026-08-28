using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageIdentity;

namespace ShilpoHubBD.Application.Validators.HeritageIdentity;

public class HeritageCertificationInputValidator : AbstractValidator<HeritageCertificationInput>
{
    public HeritageCertificationInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IssuingBody).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IssuedYear).InclusiveBetween(1700, 2100);
        RuleFor(x => x.ExpiryYear).InclusiveBetween(1700, 2200).When(x => x.ExpiryYear.HasValue);
        RuleFor(x => x.CertificateNumber).MaximumLength(100);
        RuleFor(x => x.CertificateUrl).MaximumLength(500);
    }
}
