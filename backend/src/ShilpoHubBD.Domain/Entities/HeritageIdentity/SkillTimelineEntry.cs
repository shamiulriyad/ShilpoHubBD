namespace ShilpoHubBD.Domain.Entities.HeritageIdentity;

public class SkillTimelineEntry
{
    public Guid Id { get; set; }

    public Guid ProducerHeritageIdentityId { get; set; }
    public ProducerHeritageIdentity ProducerHeritageIdentity { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Year { get; set; }
    public int DisplayOrder { get; set; }
}
