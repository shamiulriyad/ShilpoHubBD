using ShilpoHubBD.Application.DTOs.Marketplace;

namespace ShilpoHubBD.Application.DTOs.ArVr;

public class ArCraftOriginDto
{
    public string Origin { get; set; } = string.Empty;
    public int Since { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<StoryChapterDto> Chapters { get; set; } = new();
}
