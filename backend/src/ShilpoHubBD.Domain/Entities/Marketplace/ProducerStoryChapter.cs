namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class ProducerStoryChapter
{
    public Guid Id { get; set; }
    public string Heading { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Guid ProducerStoryId { get; set; }
    public ProducerStory ProducerStory { get; set; } = null!;
}
