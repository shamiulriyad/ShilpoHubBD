namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class ProductImage
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
