namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProductViewDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ViewCount { get; set; }
}
