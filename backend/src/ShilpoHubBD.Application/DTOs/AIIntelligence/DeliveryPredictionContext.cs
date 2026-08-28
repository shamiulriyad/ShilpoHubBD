namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class DeliveryPredictionContext
{
    public string ProducerName { get; set; } = string.Empty;
    public List<double> HistoricalDeliveryDays { get; set; } = new();
    public int? RequestedQuantity { get; set; }
    public int EstimatedProductionCapacity { get; set; }
}
