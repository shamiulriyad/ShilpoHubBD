using FluentValidation;
using ShilpoHubBD.Application.DTOs.PerformingArts;
using ShilpoHubBD.Application.Services.PerformingArts;

namespace ShilpoHubBD.Application.Validators.PerformingArts;

public class SavePerformingArtRequestValidator : AbstractValidator<SavePerformingArtRequest>
{
    private const string BanglaMessage = "{PropertyName} must be written in Bangla script.";

    public SavePerformingArtRequestValidator()
    {
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(120)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug may only contain lowercase letters, numbers and single hyphens.")
            .Must(s => !PerformingArtRules.ReservedSlugs.Contains(PerformingArtRules.NormaliseSlug(s ?? ""))).WithMessage("This slug is reserved.");
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TitleBn).MaximumLength(200).Must(PerformingArtRules.ContainsBangla).When(x => !string.IsNullOrWhiteSpace(x.TitleBn)).WithMessage(BanglaMessage);
        RuleFor(x => x.AlternateNames).MaximumLength(500);
        RuleFor(x => x.Category).IsInEnum();

        RuleFor(x => x.SummaryEn).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.SummaryBn).MaximumLength(2000).Must(PerformingArtRules.ContainsBangla).When(x => !string.IsNullOrWhiteSpace(x.SummaryBn)).WithMessage(BanglaMessage);
        RuleFor(x => x.DescriptionEn).MaximumLength(8000);
        RuleFor(x => x.DescriptionBn).MaximumLength(8000).Must(PerformingArtRules.ContainsBangla).When(x => !string.IsNullOrWhiteSpace(x.DescriptionBn)).WithMessage(BanglaMessage);
        RuleFor(x => x.RegionalContextEn).MaximumLength(4000);
        RuleFor(x => x.RegionalContextBn).MaximumLength(4000).Must(PerformingArtRules.ContainsBangla).When(x => !string.IsNullOrWhiteSpace(x.RegionalContextBn)).WithMessage(BanglaMessage);

        RuleFor(x => x.Division).Must(d => PerformingArtRules.Divisions.Contains(d!)).When(x => !string.IsNullOrWhiteSpace(x.Division))
            .WithMessage("Division must be one of: " + string.Join(", ", PerformingArtRules.Divisions) + ".");
        RuleFor(x => x.Upazila).MaximumLength(120);
        RuleFor(x => x.Communities).MaximumLength(1000);
        RuleFor(x => x.Artists).MaximumLength(4000);
        RuleFor(x => x.Instruments).MaximumLength(1000);
        RuleFor(x => x.ViabilityNote).MaximumLength(1000);
        RuleFor(x => x.InventoryReference).MaximumLength(100);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Sources).NotNull().Must(s => s.Count <= PerformingArtRules.MaxSources).WithMessage($"A record may cite at most {PerformingArtRules.MaxSources} sources.");
        RuleForEach(x => x.Sources).SetValidator(new SavePerformingArtSourceRequestValidator());
        RuleFor(x => x.Media).NotNull().Must(m => m.Count <= PerformingArtRules.MaxMedia).WithMessage($"A record may hold at most {PerformingArtRules.MaxMedia} media items.");
        RuleForEach(x => x.Media).SetValidator(new SavePerformingArtMediaRequestValidator());
    }
}

public class SavePerformingArtSourceRequestValidator : AbstractValidator<SavePerformingArtSourceRequest>
{
    public SavePerformingArtSourceRequestValidator()
    {
        RuleFor(x => x.SourceType).IsInEnum();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Publisher).MaximumLength(300);
        RuleFor(x => x.Url).MaximumLength(1000).Must(PerformingArtRules.IsWebUrl).When(x => !string.IsNullOrWhiteSpace(x.Url))
            .WithMessage("Source URL must be an absolute http(s) address.");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public class SavePerformingArtMediaRequestValidator : AbstractValidator<SavePerformingArtMediaRequest>
{
    public SavePerformingArtMediaRequestValidator()
    {
        RuleFor(x => x.MediaType).IsInEnum();
        RuleFor(x => x.Url).NotEmpty().MaximumLength(1000)
            .Must((media, url) => PerformingArtRules.IsValidMediaUrl(media.MediaType, url))
            .WithMessage("Media URL must be an https address or an /uploads/ path whose file type matches the media type (image: jpg/png/webp/gif, audio: mp3/ogg/wav/m4a/aac, video: mp4/webm or YouTube/Vimeo).");
        RuleFor(x => x.CaptionEn).MaximumLength(500);
        RuleFor(x => x.CaptionBn).MaximumLength(500).Must(PerformingArtRules.ContainsBangla).When(x => !string.IsNullOrWhiteSpace(x.CaptionBn))
            .WithMessage("{PropertyName} must be written in Bangla script.");
        RuleFor(x => x.Credit).NotEmpty().WithMessage("Every media item needs a credit line.").MaximumLength(300);
        RuleFor(x => x.License).MaximumLength(200);
        RuleFor(x => x.SourceUrl).MaximumLength(1000).Must(PerformingArtRules.IsWebUrl).When(x => !string.IsNullOrWhiteSpace(x.SourceUrl))
            .WithMessage("Media source URL must be an absolute http(s) address.");
    }
}

public class PerformingArtWorkflowRequestValidator : AbstractValidator<PerformingArtWorkflowRequest>
{
    public PerformingArtWorkflowRequestValidator()
    {
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
