using FluentValidation;
using ShilpoHubBD.Application.DTOs.Security;

namespace ShilpoHubBD.Application.Validators.Security;

public class CreateApiKeyRequestValidator : AbstractValidator<CreateApiKeyRequest>
{
    public CreateApiKeyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
