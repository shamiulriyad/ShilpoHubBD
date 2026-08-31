using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Domain.Entities.ArVr;

public class CulturalStory
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid? HeritagePlaceId { get; set; }
    public HeritagePlace? HeritagePlace { get; set; }

    public ICollection<CulturalStoryChapter> Chapters { get; set; } = new List<CulturalStoryChapter>();
}
