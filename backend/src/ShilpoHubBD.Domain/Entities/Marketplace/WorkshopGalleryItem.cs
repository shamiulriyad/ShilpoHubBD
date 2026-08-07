namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class WorkshopGalleryItem
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public WorkshopMediaType MediaType { get; set; }
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
