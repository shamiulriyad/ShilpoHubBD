using FluentValidation;
using ShilpoHubBD.Application.DTOs.QRVerification;

namespace ShilpoHubBD.Application.Validators.QRVerification;

public class QRVerificationQueryParametersValidator : AbstractValidator<QRVerificationQueryParameters>
{
    public QRVerificationQueryParametersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 50);
    }
}
