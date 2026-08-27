using FluentValidation;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Validators.FieldResearch;

public class UpdateFieldEvidenceRequestValidator : AbstractValidator<UpdateFieldEvidenceRequest>
{
    public UpdateFieldEvidenceRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.FileUrl).MaximumLength(2048);
        RuleFor(x => x.FileName).MaximumLength(400);
        RuleFor(x => x.MimeType).MaximumLength(150);
        RuleFor(x => x.TranscriptText).MaximumLength(32000);
        RuleFor(x => x.Language).MaximumLength(20);
        RuleFor(x => x.FileSizeBytes).GreaterThanOrEqualTo(0).When(x => x.FileSizeBytes.HasValue);
        RuleFor(x => x.DurationSeconds).GreaterThanOrEqualTo(0).When(x => x.DurationSeconds.HasValue);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        RuleFor(x => x.EvidenceType)
            .NotEmpty()
            .Must(t => Enum.TryParse<FieldEvidenceType>(t, true, out _))
            .WithMessage("EvidenceType is not a valid field evidence type.");
    }
}
