using FluentValidation;
using ShilpoHubBD.Application.DTOs.BusinessPartner;

namespace ShilpoHubBD.Application.Validators.BusinessPartner;

public class BusinessDocumentInputValidator : AbstractValidator<BusinessDocumentInput>
{
    public BusinessDocumentInputValidator()
    {
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.DocumentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.DocumentNumber).MaximumLength(100);
        RuleFor(x => x.ExpiryDate)
            .GreaterThanOrEqualTo(x => x.IssuedDate)
            .When(x => x.IssuedDate.HasValue && x.ExpiryDate.HasValue)
            .WithMessage("ExpiryDate must be on or after IssuedDate.");
    }
}
