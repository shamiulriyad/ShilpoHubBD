namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class CreateProductVariantRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public int Stock { get; set; }
    public int DisplayOrder { get; set; }
}
