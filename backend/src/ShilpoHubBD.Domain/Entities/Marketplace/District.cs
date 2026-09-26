namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class District
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string? Description { get; set; }
    public string? KnownFor { get; set; }
    public string? SourceUrl { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageCredit { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
