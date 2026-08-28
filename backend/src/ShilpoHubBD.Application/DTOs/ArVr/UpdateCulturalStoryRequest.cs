namespace ShilpoHubBD.Application.DTOs.ArVr;

public class UpdateCulturalStoryRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? HeritagePlaceId { get; set; }
    public List<CulturalStoryChapterInput> Chapters { get; set; } = new();
}
