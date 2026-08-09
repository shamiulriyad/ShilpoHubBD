namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class DeliveryPredictionRequest
{
    public Guid ProducerId { get; set; }
    public int? Quantity { get; set; }
}
