using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Validators.HeritageDatabase;

public class CreateHeritageDatasetExportRequestValidator : AbstractValidator<CreateHeritageDatasetExportRequest>
{
    public CreateHeritageDatasetExportRequestValidator()
    {
        RuleFor(x => x.FilterJson).MaximumLength(4000);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Format)
            .Must(v => string.IsNullOrWhiteSpace(v) || Enum.TryParse<HeritageDatasetFileFormat>(v, true, out _))
            .WithMessage("Format must be one of: None, Csv, Json, GeoJson, Xlsx.");
    }
}
