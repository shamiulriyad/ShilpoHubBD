namespace ShilpoHubBD.Application.DTOs.AITourism;

public class BudgetPlanRequest
{
    public List<BudgetSelectionDto> Selections { get; set; } = new();
    public int DurationDays { get; set; } = 1;
    public int PartySize { get; set; } = 1;
    public decimal? DailyFoodBudgetPerPerson { get; set; }
    public decimal? DailyMiscBudgetPerPerson { get; set; }
}
