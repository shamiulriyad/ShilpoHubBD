using ShilpoHubBD.Domain.Entities.ArVr;

namespace ShilpoHubBD.Application.DTOs.ArVr;

public class UpdateVillageTourStopRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public ArVrMediaType MediaType { get; set; } = ArVrMediaType.Image360;
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
