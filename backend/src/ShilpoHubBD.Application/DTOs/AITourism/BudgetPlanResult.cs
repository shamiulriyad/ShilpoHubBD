namespace ShilpoHubBD.Application.DTOs.AITourism;

public class BudgetPlanResult
{
    public List<BudgetLineItemDto> LineItems { get; set; } = new();
    public decimal TotalEstimatedCost { get; set; }
    public decimal PerPersonCost { get; set; }
    public string Notes { get; set; } = string.Empty;
}
