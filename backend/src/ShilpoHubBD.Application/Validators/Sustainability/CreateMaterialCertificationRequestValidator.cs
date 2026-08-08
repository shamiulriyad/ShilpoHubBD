using FluentValidation;
using ShilpoHubBD.Application.DTOs.Sustainability;

namespace ShilpoHubBD.Application.Validators.Sustainability;

public class CreateMaterialCertificationRequestValidator : AbstractValidator<CreateMaterialCertificationRequest>
{
    public CreateMaterialCertificationRequestValidator()
    {
        RuleFor(x => x.MaterialName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CertifyingBody).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CertificateReference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.IssuedAt).NotEmpty();
        RuleFor(x => x.ExpiresAt)
            .GreaterThan(x => x.IssuedAt)
            .When(x => x.ExpiresAt.HasValue)
            .WithMessage("ExpiresAt must be after IssuedAt.");
    }
}
