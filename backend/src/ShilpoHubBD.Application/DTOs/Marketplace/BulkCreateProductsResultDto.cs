namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class BulkCreateProductsResultDto
{
    public List<ProductDto> Created { get; set; } = new();
    public List<BulkProductErrorDto> Errors { get; set; } = new();
}
