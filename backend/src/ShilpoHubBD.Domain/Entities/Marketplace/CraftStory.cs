namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class CraftStory
{
    public Guid Id { get; set; }
    public string Origin { get; set; } = string.Empty;
    public int Since { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<CraftStoryChapter> Chapters { get; set; } = new List<CraftStoryChapter>();
}
