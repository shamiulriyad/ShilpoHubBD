namespace ShilpoHubBD.Application.DTOs.AITourism;

public class BudgetPlanResult
{
    public List<BudgetLineItemDto> LineItems { get; set; } = new();
    public decimal TotalEstimatedCost { get; set; }
    public decimal PerPersonCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    // Costs that exist in the trip but have no verified figure -- excluded from the total, named here.
    public List<string> UnverifiedCosts { get; set; } = new();
    // Rough assumption-based items for the costs above that have no verified figure, and the total
    // when they are added to the verified one. Always presented as an estimate, not as fact.
    public List<BudgetLineItemDto> EstimatedItems { get; set; } = new();
    public decimal EstimatedTotal { get; set; }
    public decimal EstimatedPerPerson { get; set; }
    public string? EstimateNote { get; set; }
}
