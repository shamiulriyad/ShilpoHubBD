namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class PriceForecastRequest
{
    public Guid CategoryId { get; set; }
    public int HorizonMonths { get; set; } = 3;
}
