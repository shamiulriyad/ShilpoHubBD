using FluentValidation;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;

namespace ShilpoHubBD.Application.Validators.CSRSponsorship;

public class AddProgressUpdateRequestValidator : AbstractValidator<AddProgressUpdateRequest>
{
    public AddProgressUpdateRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MaximumLength(2000);
    }
}
