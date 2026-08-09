namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class SupplierPerformanceDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int TotalProcurements { get; set; }
    public int CompletedProcurements { get; set; }
    public int CancelledProcurements { get; set; }
    public decimal TotalProcurementValue { get; set; }
    public double? AverageDeliveryDays { get; set; }
}
