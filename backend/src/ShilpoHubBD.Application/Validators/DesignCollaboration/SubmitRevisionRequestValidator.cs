using FluentValidation;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;

namespace ShilpoHubBD.Application.Validators.DesignCollaboration;

public class SubmitRevisionRequestValidator : AbstractValidator<SubmitRevisionRequest>
{
    public SubmitRevisionRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleForEach(x => x.Files).SetValidator(new DesignFileInputValidator());
    }
}
