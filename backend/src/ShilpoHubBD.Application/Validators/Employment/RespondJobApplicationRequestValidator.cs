using FluentValidation;
using ShilpoHubBD.Application.DTOs.Employment;

namespace ShilpoHubBD.Application.Validators.Employment;

public class RespondJobApplicationRequestValidator : AbstractValidator<RespondJobApplicationRequest>
{
    public RespondJobApplicationRequestValidator()
    {
        RuleFor(x => x.ResponseMessage).MaximumLength(2000);
    }
}
