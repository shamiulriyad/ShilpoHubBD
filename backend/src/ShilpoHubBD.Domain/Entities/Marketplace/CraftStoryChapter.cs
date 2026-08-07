namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class CraftStoryChapter
{
    public Guid Id { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Guid CraftStoryId { get; set; }
    public CraftStory CraftStory { get; set; } = null!;
}
