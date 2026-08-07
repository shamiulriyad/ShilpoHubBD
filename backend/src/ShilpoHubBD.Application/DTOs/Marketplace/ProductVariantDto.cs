namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}
