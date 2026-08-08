using ShilpoHubBD.Application.DTOs.ProducerBusiness;

namespace ShilpoHubBD.Application.DTOs.AIBusiness;

public class SalesInsightsContext
{
    public RevenueDashboardDto Revenue { get; set; } = new();
    public SalesAnalyticsDto Sales { get; set; } = new();
    public List<ProductPerformanceDto> ProductPerformance { get; set; } = new();
}
