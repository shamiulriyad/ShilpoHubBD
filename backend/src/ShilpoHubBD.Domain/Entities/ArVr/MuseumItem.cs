using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.ArVr;

public class MuseumItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Era { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string? ModelUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public ICollection<MuseumItemMedia> Media { get; set; } = new List<MuseumItemMedia>();
}
