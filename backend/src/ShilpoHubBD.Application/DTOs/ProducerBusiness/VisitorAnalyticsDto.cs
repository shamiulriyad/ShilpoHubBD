namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class VisitorAnalyticsDto
{
    public int TotalViews { get; set; }
    public List<ProductViewDto> ProductViews { get; set; } = new();
}
