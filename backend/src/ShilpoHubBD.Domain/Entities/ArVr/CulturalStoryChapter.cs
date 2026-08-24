namespace ShilpoHubBD.Domain.Entities.ArVr;

public class CulturalStoryChapter
{
    public Guid Id { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public ArVrMediaType? MediaType { get; set; }
    public int DisplayOrder { get; set; }

    public Guid CulturalStoryId { get; set; }
    public CulturalStory CulturalStory { get; set; } = null!;
}
