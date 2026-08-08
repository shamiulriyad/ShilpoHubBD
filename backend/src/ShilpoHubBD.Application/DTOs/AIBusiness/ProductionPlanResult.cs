namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class ProductionPlanResult
{
    public int UnitsToProduce { get; set; }
    public int EstimatedDaysNeeded { get; set; }
    public DateTime RecommendedStartDate { get; set; }
    public DateTime EstimatedCompletionDate { get; set; }
    public List<ProductionScheduleEntryDto> Schedule { get; set; } = new();
    public string Notes { get; set; } = string.Empty;
}
