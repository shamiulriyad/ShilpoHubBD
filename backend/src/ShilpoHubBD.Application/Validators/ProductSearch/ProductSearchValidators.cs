using FluentValidation;
using ShilpoHubBD.Application.DTOs.ProductSearch;

namespace ShilpoHubBD.Application.Validators.ProductSearch;

public class SaveLookupItemRequestValidator : AbstractValidator<SaveLookupItemRequest>
{
    public SaveLookupItemRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.NameBn).MaximumLength(120);
        RuleFor(x => x.Slug).MaximumLength(120).Matches("^[a-zA-Z0-9-]+$").When(x => !string.IsNullOrWhiteSpace(x.Slug))
            .WithMessage("Slug may only contain letters, numbers and hyphens.");
    }
}

public class SaveProductAttributesRequestValidator : AbstractValidator<SaveProductAttributesRequest>
{
    public static readonly string[] ProductionMethods = { "Handmade", "Handloom", "Hand-finished" };

    public SaveProductAttributesRequestValidator()
    {
        RuleFor(x => x.MaterialIds).Must(ids => ids.Count <= 15).WithMessage("Choose at most 15 materials.");
        RuleFor(x => x.Tags).Must(BeShortList).WithMessage("Up to 30 tags of at most 60 characters each.");
        RuleFor(x => x.Keywords).Must(BeShortList).WithMessage("Up to 30 keywords of at most 60 characters each.");
        RuleFor(x => x.Occasions).Must(BeShortList).WithMessage("Up to 30 occasions of at most 60 characters each.");
        RuleFor(x => x.Colors).Must(BeShortList).WithMessage("Up to 30 colours of at most 60 characters each.");
        RuleFor(x => x.CraftTechnique).MaximumLength(200);
        RuleFor(x => x.ProductionMethod).Must(m => string.IsNullOrWhiteSpace(m) || ProductionMethods.Contains(m.Trim(), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Production method must be Handmade, Handloom or Hand-finished.");
        RuleFor(x => x.DimensionsText).MaximumLength(120);
        RuleFor(x => x.LengthCm).InclusiveBetween(0m, 100000m).When(x => x.LengthCm.HasValue);
        RuleFor(x => x.WidthCm).InclusiveBetween(0m, 100000m).When(x => x.WidthCm.HasValue);
        RuleFor(x => x.HeightCm).InclusiveBetween(0m, 100000m).When(x => x.HeightCm.HasValue);
        RuleFor(x => x.WeightGrams).InclusiveBetween(0m, 10000000m).When(x => x.WeightGrams.HasValue);
        RuleFor(x => x.LeadTimeDays).InclusiveBetween(0, 365).When(x => x.LeadTimeDays.HasValue);
        RuleFor(x => x.CareInstructions).MaximumLength(500);
    }

    private static bool BeShortList(List<string> values) => values.Count <= 30 && values.All(v => (v ?? string.Empty).Trim().Length <= 60);
}

public class ConfirmAttributeSuggestionRequestValidator : AbstractValidator<ConfirmAttributeSuggestionRequest>
{
    public ConfirmAttributeSuggestionRequestValidator()
    {
        RuleFor(x => x.Attributes).NotNull().SetValidator(new SaveProductAttributesRequestValidator());
    }
}

public class SubmitAttributeSuggestionRequestValidator : AbstractValidator<SubmitAttributeSuggestionRequest>
{
    public SubmitAttributeSuggestionRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Model).NotEmpty().MaximumLength(100);
    }
}
