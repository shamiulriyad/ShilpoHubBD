using FluentValidation;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Validators.FieldResearch;

public class UpdateSurveyStatusRequestValidator : AbstractValidator<UpdateSurveyStatusRequest>
{
    public UpdateSurveyStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<SurveyStatus>(s, true, out _))
            .WithMessage("Status must be one of: Draft, Active, Paused, Closed, Archived.");
    }
}
