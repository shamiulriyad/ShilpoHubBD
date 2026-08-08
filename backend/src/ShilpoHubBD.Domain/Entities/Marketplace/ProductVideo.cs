namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class ProductVideo
{
    public Guid Id { get; set; }
    public string VideoUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
