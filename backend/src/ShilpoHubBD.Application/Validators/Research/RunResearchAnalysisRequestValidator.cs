using FluentValidation;
using ShilpoHubBD.Application.DTOs.Research;

namespace ShilpoHubBD.Application.Validators.Research;

public class RunResearchAnalysisRequestValidator : AbstractValidator<RunResearchAnalysisRequest>
{
    public RunResearchAnalysisRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(4000);

        RuleFor(x => x.ResearchQuestions).NotNull();
        RuleFor(x => x.ResearchQuestions.Count).LessThanOrEqualTo(25)
            .WithMessage("At most 25 research questions can be submitted at once.");
        RuleForEach(x => x.ResearchQuestions).MaximumLength(500);

        RuleFor(x => x.SelectedData).NotNull();
        RuleFor(x => x.SelectedData.Count).LessThanOrEqualTo(2000)
            .WithMessage("At most 2000 selected data points can be submitted at once.");
        RuleForEach(x => x.SelectedData).ChildRules(point =>
        {
            point.RuleFor(p => p.Label).NotEmpty().MaximumLength(200);
            point.RuleFor(p => p.Series).MaximumLength(120);
            point.RuleFor(p => p.Category).MaximumLength(120);
        });

        RuleFor(x => x.RangeEnd)
            .GreaterThanOrEqualTo(x => x.RangeStart!.Value)
            .When(x => x.RangeStart.HasValue && x.RangeEnd.HasValue)
            .WithMessage("RangeEnd cannot be earlier than RangeStart.");
    }
}
