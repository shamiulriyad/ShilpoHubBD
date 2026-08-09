using FluentValidation;
using ShilpoHubBD.Application.DTOs.Contracts;

namespace ShilpoHubBD.Application.Validators.Contracts;

public class AddContractDocumentRequestValidator : AbstractValidator<AddContractDocumentRequest>
{
    public AddContractDocumentRequestValidator()
    {
        RuleFor(x => x.DocumentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DocumentType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(500);
    }
}
