using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Domain.Entities.ArVr;

public class VillageTourStop
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public ArVrMediaType MediaType { get; set; } = ArVrMediaType.Image360;
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid HeritagePlaceId { get; set; }
    public HeritagePlace HeritagePlace { get; set; } = null!;
}
