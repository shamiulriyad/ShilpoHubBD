namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class ProducerStoryDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string HeritageId { get; set; } = string.Empty;
    public int Generations { get; set; }
    public int? FoundingYear { get; set; }
    public string Quote { get; set; } = string.Empty;
    public List<StoryChapterDto> Chapters { get; set; } = new();
}
