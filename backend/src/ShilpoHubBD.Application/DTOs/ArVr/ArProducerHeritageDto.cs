using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.DTOs.ArVr;

public class ArProducerHeritageDto
{
    public string HeritageId { get; set; } = string.Empty;
    public int Generations { get; set; }
    public int? FoundingYear { get; set; }
    public string Quote { get; set; } = string.Empty;
    public List<StoryChapterDto> Chapters { get; set; } = new();
}
