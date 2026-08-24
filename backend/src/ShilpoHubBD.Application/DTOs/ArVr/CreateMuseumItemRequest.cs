namespace ShilpoHubBD.Application.DTOs.ArVr;

public class CreateMuseumItemRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Era { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string? ModelUrl { get; set; }
    public bool IsFeatured { get; set; }
    public Guid DistrictId { get; set; }
    public Guid? ProductId { get; set; }
    public List<MediaInput> Media { get; set; } = new();
}
