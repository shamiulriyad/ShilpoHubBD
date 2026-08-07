namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class CraftStoryDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public int Since { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<StoryChapterDto> Chapters { get; set; } = new();
}
