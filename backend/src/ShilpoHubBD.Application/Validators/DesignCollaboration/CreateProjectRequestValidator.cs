using FluentValidation;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;

namespace ShilpoHubBD.Application.Validators.DesignCollaboration;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.ProducerId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DesignRequirements).NotEmpty().MaximumLength(4000);
        RuleForEach(x => x.InitialFiles).SetValidator(new DesignFileInputValidator());
    }
}
