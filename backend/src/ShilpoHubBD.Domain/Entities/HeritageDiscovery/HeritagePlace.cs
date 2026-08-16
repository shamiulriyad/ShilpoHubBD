using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.HeritageDiscovery;

public class HeritagePlace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public HeritagePlaceType PlaceType { get; set; }
    public string? Address { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;

    public ICollection<HeritageFestival> Festivals { get; set; } = new List<HeritageFestival>();
    public ICollection<CulturalEvent> Events { get; set; } = new List<CulturalEvent>();
    public ICollection<LocalCuisine> CuisineItems { get; set; } = new List<LocalCuisine>();
}
