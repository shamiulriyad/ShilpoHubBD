using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;

namespace ShilpoHubBD.Application.Validators.ProductDevelopment;

public class SubmitPrototypeRequestValidator : AbstractValidator<SubmitPrototypeRequest>
{
    public SubmitPrototypeRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.Files).SetValidator(new PrototypeFileInputValidator());
    }
}
