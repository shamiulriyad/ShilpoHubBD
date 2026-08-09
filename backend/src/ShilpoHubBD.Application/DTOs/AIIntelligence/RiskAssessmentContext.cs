namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

public class RiskAssessmentContext
{
    public string ProducerName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalOrderItemCount { get; set; }
    public int CancelledOrderItemCount { get; set; }
    public int TotalQuotationResponseCount { get; set; }
    public int RejectedQuotationResponseCount { get; set; }
    public int TotalProcurementCount { get; set; }
    public int CancelledProcurementCount { get; set; }
    public bool HasVerifiedCertification { get; set; }
}
