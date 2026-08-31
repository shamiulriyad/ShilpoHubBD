using FluentValidation;
using ShilpoHubBD.Application.DTOs.Apprenticeship;

namespace ShilpoHubBD.Application.Validators.Apprenticeship;

public class RespondProgramApplicationRequestValidator : AbstractValidator<RespondProgramApplicationRequest>
{
    public RespondProgramApplicationRequestValidator()
    {
        RuleFor(x => x.ResponseMessage).MaximumLength(2000);
    }
}
