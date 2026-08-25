using FluentValidation;
using ShilpoHubBD.Application.DTOs.Apprenticeship;

namespace ShilpoHubBD.Application.Validators.Apprenticeship;

public class CreateProgramApplicationRequestValidator : AbstractValidator<CreateProgramApplicationRequest>
{
    public CreateProgramApplicationRequestValidator()
    {
        RuleFor(x => x.ProgramId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}
