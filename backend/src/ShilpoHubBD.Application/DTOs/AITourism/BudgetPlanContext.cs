namespace ShilpoHubBD.Application.DTOs.AITourism;

public class BudgetPlanContext
{
    public List<BudgetServiceLineDto> ServiceLines { get; set; } = new();
    public int DurationDays { get; set; }
    public int PartySize { get; set; }
    public decimal DailyFoodBudgetPerPerson { get; set; }
    public decimal DailyMiscBudgetPerPerson { get; set; }
}
