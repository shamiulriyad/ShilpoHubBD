using FluentValidation;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Domain.Entities.Admin;

namespace ShilpoHubBD.Application.Validators.Admin;

public class SubmitIdentityVerificationRequestValidator : AbstractValidator<SubmitIdentityVerificationRequest>
{
    public SubmitIdentityVerificationRequestValidator()
    {
        RuleFor(x => x.Type).NotEmpty().Must(BeEnum<IdentityVerificationType>).WithMessage("Invalid Type.");
        RuleFor(x => x.DocumentNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FrontImageUrl).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.BackImageUrl).MaximumLength(2000);
        RuleFor(x => x.SelfieImageUrl).MaximumLength(2000);
        RuleFor(x => x.ApplicantNote).MaximumLength(1000);
    }

    private static bool BeEnum<T>(string v) where T : struct, Enum => Enum.TryParse<T>(v, true, out _);
}

public class RejectIdentityVerificationRequestValidator : AbstractValidator<RejectIdentityVerificationRequest>
{
    public RejectIdentityVerificationRequestValidator()
        => RuleFor(x => x.RejectionReason).NotEmpty().MaximumLength(1000);
}
