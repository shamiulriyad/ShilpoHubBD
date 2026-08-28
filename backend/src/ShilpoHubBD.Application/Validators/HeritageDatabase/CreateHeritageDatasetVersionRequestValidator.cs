using FluentValidation;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Application.Validators.HeritageDatabase;

public class CreateHeritageDatasetVersionRequestValidator : AbstractValidator<CreateHeritageDatasetVersionRequest>
{
    public CreateHeritageDatasetVersionRequestValidator()
    {
        RuleFor(x => x.Changelog).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Label).MaximumLength(50);
        RuleFor(x => x.SourceFileName).MaximumLength(260);
        RuleFor(x => x.SourceFileUrl).MaximumLength(2048);
        RuleFor(x => x.SourceContentHash).MaximumLength(128);
        RuleFor(x => x.ImportNotes).MaximumLength(2000);
        RuleFor(x => x.SchemaJson).MaximumLength(16000);
        RuleFor(x => x.RecordCount).GreaterThanOrEqualTo(0).When(x => x.RecordCount.HasValue);
        RuleFor(x => x.ImportedRowCount).GreaterThanOrEqualTo(0).When(x => x.ImportedRowCount.HasValue);
        RuleFor(x => x.ImportErrorCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Format)
            .Must(v => string.IsNullOrWhiteSpace(v) || Enum.TryParse<HeritageDatasetFileFormat>(v, true, out _))
            .WithMessage("Format must be one of: None, Csv, Json, GeoJson, Xlsx.");
    }
}
