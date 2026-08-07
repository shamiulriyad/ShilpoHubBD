namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class District
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
