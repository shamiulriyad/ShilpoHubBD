namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProductPerformanceDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int SalesCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public decimal ConversionRate { get; set; }
}
