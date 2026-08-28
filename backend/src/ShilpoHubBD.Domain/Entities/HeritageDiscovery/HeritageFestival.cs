using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.HeritageDiscovery;

public class HeritageFestival
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsRecurringAnnually { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;

    public Guid? HeritagePlaceId { get; set; }
    public HeritagePlace? HeritagePlace { get; set; }
}
