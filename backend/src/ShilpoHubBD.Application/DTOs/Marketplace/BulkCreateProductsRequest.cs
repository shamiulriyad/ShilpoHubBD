namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class BulkCreateProductsRequest
{
    public List<CreateProductRequest> Products { get; set; } = new();
}
