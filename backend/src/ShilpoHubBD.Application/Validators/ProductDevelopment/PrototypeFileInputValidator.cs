using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;

namespace ShilpoHubBD.Application.Validators.ProductDevelopment;

public class PrototypeFileInputValidator : AbstractValidator<PrototypeFileInput>
{
    public PrototypeFileInputValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FileType).NotEmpty().MaximumLength(50);
    }
}
