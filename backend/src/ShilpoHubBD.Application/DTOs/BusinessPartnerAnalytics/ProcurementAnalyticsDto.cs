namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class ProcurementAnalyticsDto
{
    public int TotalRequests { get; set; }
    public decimal TotalValue { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int ConvertedCount { get; set; }
    public double? AverageApprovalDays { get; set; }
    public List<ProcurementStatusBreakdownDto> StatusBreakdown { get; set; } = new();
}
