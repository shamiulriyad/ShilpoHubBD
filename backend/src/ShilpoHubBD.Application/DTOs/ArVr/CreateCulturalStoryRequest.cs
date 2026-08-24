namespace ShilpoHubBD.Application.DTOs.ArVr;

public class CreateCulturalStoryRequest
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public Guid? HeritagePlaceId { get; set; }
    public List<CulturalStoryChapterInput> Chapters { get; set; } = new();
}
