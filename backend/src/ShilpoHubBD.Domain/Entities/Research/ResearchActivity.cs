using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>Append-only audit trail of meaningful actions inside a research project.</summary>
public class ResearchActivity
{
    public Guid Id { get; set; }

    public Guid ResearchProjectId { get; set; }
    public ResearchProject Project { get; set; } = null!;

    public Guid ActorUserId { get; set; }
    public User Actor { get; set; } = null!;

    public ResearchActivityType Type { get; set; }
    public string Summary { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
