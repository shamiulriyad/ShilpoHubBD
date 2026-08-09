namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

// Consolidated signal set for one producer, assembled from existing Product/Order/Quotation/
// Procurement/Certification data. Feeds the Quality/Delivery/Risk context builders.
public class ProducerIntelligenceProfileDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int ProductCount { get; set; }
    public int HandmadeVerifiedProductCount { get; set; }
    public int EstimatedProductionCapacity { get; set; }
    public int CertificationCount { get; set; }
    public bool HasVerifiedCertification { get; set; }

    public int TotalOrderItemCount { get; set; }
    public int DeliveredOrderItemCount { get; set; }
    public int CancelledOrderItemCount { get; set; }
    public List<double> HistoricalDeliveryDays { get; set; } = new();

    public int TotalQuotationResponseCount { get; set; }
    public int RejectedQuotationResponseCount { get; set; }

    public int TotalProcurementCount { get; set; }
    public int CancelledProcurementCount { get; set; }
}
