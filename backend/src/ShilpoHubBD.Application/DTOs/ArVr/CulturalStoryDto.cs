namespace ShilpoHubBD.Application.DTOs.ArVr;

public class CulturalStoryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public string? HeritagePlaceName { get; set; }
    public List<CulturalStoryChapterDto> Chapters { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
