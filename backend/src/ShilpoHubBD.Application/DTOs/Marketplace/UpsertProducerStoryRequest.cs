namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class UpsertProducerStoryRequest
{
    public int Generations { get; set; }
    public int? FoundingYear { get; set; }
    public string Quote { get; set; } = string.Empty;
    public List<StoryChapterInput> Chapters { get; set; } = new();
}
