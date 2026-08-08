namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class ProductionScheduleEntryDto
{
    public DateTime Date { get; set; }
    public int PlannedUnits { get; set; }
    public int CumulativeUnits { get; set; }
}
