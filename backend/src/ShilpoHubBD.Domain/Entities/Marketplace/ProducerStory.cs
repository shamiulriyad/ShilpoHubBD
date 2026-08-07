namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class ProducerStory
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }

    // Family heritage: how far back the craft traces in this producer's family, and their public heritage registry id.
    public string HeritageId { get; set; } = string.Empty;
    public int Generations { get; set; }
    public int? FoundingYear { get; set; }

    public string Quote { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProducerStoryChapter> Chapters { get; set; } = new List<ProducerStoryChapter>();
}
