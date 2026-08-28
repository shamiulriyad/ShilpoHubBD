using FluentValidation;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Validators.Governance;

public class GenerateGovReportRequestValidator : AbstractValidator<GenerateGovReportRequest>
{
    public GenerateGovReportRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReportType).NotEmpty()
            .Must(v => Enum.TryParse<GovReportType>(v, true, out _))
            .WithMessage("ReportType must be one of: Monthly, Quarterly, Annual, Custom.");
        RuleFor(x => x.Highlights).MaximumLength(4000);
        RuleFor(x => x.PeriodEnd)
            .GreaterThan(x => x.PeriodStart!.Value)
            .When(x => x.PeriodStart.HasValue && x.PeriodEnd.HasValue)
            .WithMessage("PeriodEnd must be after PeriodStart.");
    }
}

public class UpdateGovReportRequestValidator : AbstractValidator<UpdateGovReportRequest>
{
    public UpdateGovReportRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Highlights).MaximumLength(4000);
        RuleFor(x => x.Summary).MaximumLength(4000);
        RuleFor(x => x.Status).Must(v => Enum.TryParse<GovReportStatus>(v, true, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.Status))
            .WithMessage("Status must be one of: Draft, Published, Archived.");
    }
}

public class CreateAnalyticsExportRequestValidator : AbstractValidator<CreateAnalyticsExportRequest>
{
    public CreateAnalyticsExportRequestValidator()
    {
        RuleFor(x => x.Dataset).NotEmpty()
            .Must(v => Enum.TryParse<AnalyticsExportDataset>(v, true, out _))
            .WithMessage("Invalid Dataset.");
        RuleFor(x => x.Format).NotEmpty()
            .Must(v => Enum.TryParse<AnalyticsExportFormat>(v, true, out _))
            .WithMessage("Format must be one of: Csv, Json, Xlsx, Pdf.");
        RuleFor(x => x.FiltersJson).MaximumLength(8000);
        RuleFor(x => x.GovReportId).NotNull()
            .When(x => string.Equals(x.Dataset, "GovReport", StringComparison.OrdinalIgnoreCase))
            .WithMessage("GovReportId is required when Dataset is GovReport.");
    }
}

public class CompleteAnalyticsExportRequestValidator : AbstractValidator<CompleteAnalyticsExportRequest>
{
    public CompleteAnalyticsExportRequestValidator()
    {
        RuleFor(x => x.Outcome).NotEmpty()
            .Must(v => v.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                || v.Equals("Failed", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Outcome must be Completed or Failed.");
        RuleFor(x => x.FileUrl).MaximumLength(1000);
        RuleFor(x => x.RowCount).GreaterThanOrEqualTo(0).When(x => x.RowCount.HasValue);
        RuleFor(x => x.FileSizeBytes).GreaterThanOrEqualTo(0).When(x => x.FileSizeBytes.HasValue);
        RuleFor(x => x.FailureReason).MaximumLength(2000);
    }
}

public class GenerateGovForecastRequestValidator : AbstractValidator<GenerateGovForecastRequest>
{
    public GenerateGovForecastRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.HorizonMonths).InclusiveBetween(1, 60).When(x => x.HorizonMonths.HasValue);
    }
}
