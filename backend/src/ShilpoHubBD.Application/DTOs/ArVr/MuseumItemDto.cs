namespace ShilpoHubBD.Application.DTOs.ArVr;

public class MuseumItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Era { get; set; }
    public string CoverImageUrl { get; set; } = string.Empty;
    public string? ModelUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public List<MuseumItemMediaDto> Media { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
