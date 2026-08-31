using FluentValidation;
using ShilpoHubBD.Application.DTOs.AITourism;

namespace ShilpoHubBD.Application.Validators.AITourism;

public class BudgetPlanRequestValidator : AbstractValidator<BudgetPlanRequest>
{
    public BudgetPlanRequestValidator()
    {
        RuleFor(x => x.DurationDays).InclusiveBetween(1, 90);
        RuleFor(x => x.PartySize).InclusiveBetween(1, 100);
        RuleFor(x => x.DailyFoodBudgetPerPerson).GreaterThanOrEqualTo(0).When(x => x.DailyFoodBudgetPerPerson.HasValue);
        RuleFor(x => x.DailyMiscBudgetPerPerson).GreaterThanOrEqualTo(0).When(x => x.DailyMiscBudgetPerPerson.HasValue);

        RuleForEach(x => x.Selections).ChildRules(selection =>
        {
            selection.RuleFor(s => s.ServiceId).NotEmpty();
            selection.RuleFor(s => s.PartySize).GreaterThan(0);
        });
    }
}
