namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class AnalyticsQueryParameters
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Industry { get; set; }
    public Guid? DistrictId { get; set; }
}
