using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Research;

public class ResearchNote
{
    public Guid Id { get; set; }

    public Guid ResearchProjectId { get; set; }
    public ResearchProject Project { get; set; } = null!;

    public Guid AuthorUserId { get; set; }
    public User Author { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ResearchNoteVisibility Visibility { get; set; } = ResearchNoteVisibility.Team;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
