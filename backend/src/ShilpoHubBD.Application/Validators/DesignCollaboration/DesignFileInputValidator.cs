using FluentValidation;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;

namespace ShilpoHubBD.Application.Validators.DesignCollaboration;

public class DesignFileInputValidator : AbstractValidator<DesignFileInput>
{
    public DesignFileInputValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.FileType).NotEmpty().MaximumLength(50);
    }
}
