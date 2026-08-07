namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class UpdateCraftStoryRequest
{
    public string Origin { get; set; } = string.Empty;
    public int Since { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<StoryChapterInput> Chapters { get; set; } = new();
}
