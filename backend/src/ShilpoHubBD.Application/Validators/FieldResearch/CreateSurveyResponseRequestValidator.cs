using FluentValidation;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Validators.FieldResearch;

public class CreateSurveyResponseRequestValidator : AbstractValidator<CreateSurveyResponseRequest>
{
    public CreateSurveyResponseRequestValidator()
    {
        RuleFor(x => x.RespondentName).MaximumLength(200);
        RuleFor(x => x.RespondentContact).MaximumLength(200);
        RuleFor(x => x.VillageName).MaximumLength(200);
        RuleFor(x => x.DistrictName).MaximumLength(200);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.LocationAccuracyMeters).GreaterThanOrEqualTo(0).When(x => x.LocationAccuracyMeters.HasValue);
        RuleFor(x => x.Source)
            .Must(s => string.IsNullOrWhiteSpace(s) || Enum.TryParse<FieldResponseSource>(s, true, out _))
            .WithMessage("Source must be one of: FieldInterview, PhoneInterview, SelfReported, Import.");
        RuleFor(x => x.Answers.Count).LessThanOrEqualTo(500);
        RuleForEach(x => x.Answers).ChildRules(a =>
        {
            a.RuleFor(p => p.SurveyQuestionId).NotEmpty();
            a.RuleFor(p => p.ValueText).MaximumLength(8000);
            a.RuleFor(p => p.Latitude).InclusiveBetween(-90, 90).When(p => p.Latitude.HasValue);
            a.RuleFor(p => p.Longitude).InclusiveBetween(-180, 180).When(p => p.Longitude.HasValue);
        });
    }
}
