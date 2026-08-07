using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class CreateWorkshopGalleryItemRequest
{
    public string MediaUrl { get; set; } = string.Empty;
    public WorkshopMediaType MediaType { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}
