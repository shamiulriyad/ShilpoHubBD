namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class WorkshopGalleryItemDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}
