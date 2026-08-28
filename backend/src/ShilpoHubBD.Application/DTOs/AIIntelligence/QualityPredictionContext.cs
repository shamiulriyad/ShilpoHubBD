namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class QualityPredictionContext
{
    public string ProducerName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int ProductCount { get; set; }
    public int HandmadeVerifiedProductCount { get; set; }
    public int DeliveredOrderItemCount { get; set; }
    public int CancelledOrderItemCount { get; set; }
}
