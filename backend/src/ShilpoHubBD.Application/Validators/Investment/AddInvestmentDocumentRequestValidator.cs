using FluentValidation;
using ShilpoHubBD.Application.DTOs.Investment;

namespace ShilpoHubBD.Application.Validators.Investment;

public class AddInvestmentDocumentRequestValidator : AbstractValidator<AddInvestmentDocumentRequest>
{
    public AddInvestmentDocumentRequestValidator()
    {
        RuleFor(x => x.DocumentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(500);
    }
}
