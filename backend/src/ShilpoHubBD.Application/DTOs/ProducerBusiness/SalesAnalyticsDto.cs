namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class SalesAnalyticsDto
{
    public List<ProductSalesDto> TopProducts { get; set; } = new();
    public List<DailySalesDto> DailySales { get; set; } = new();
}
