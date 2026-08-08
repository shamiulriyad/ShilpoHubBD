namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class DemandForecastRequest
{
    public Guid ProductId { get; set; }
    public int HorizonWeeks { get; set; } = 4;
}
