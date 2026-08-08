namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class ProductionPlannerContext
{
    public string ProductName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int TargetQuantity { get; set; }
    public int DailyProductionCapacity { get; set; }
    public int LeadTimeDays { get; set; }
}
