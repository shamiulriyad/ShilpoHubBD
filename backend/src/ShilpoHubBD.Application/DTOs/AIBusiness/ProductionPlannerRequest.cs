namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class ProductionPlannerRequest
{
    public Guid ProductId { get; set; }
    public int TargetQuantity { get; set; }
    public int DailyProductionCapacity { get; set; }
    public int LeadTimeDays { get; set; } = 1;
}
