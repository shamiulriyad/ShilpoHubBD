using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>Links a <see cref="User"/> to a <see cref="ResearchProject"/> with a privilege role.</summary>
public class ResearchProjectMember
{
    public Guid Id { get; set; }

    public Guid ResearchProjectId { get; set; }
    public ResearchProject Project { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ResearchRole Role { get; set; } = ResearchRole.Contributor;

    public Guid? InvitedByUserId { get; set; }
    public User? InvitedBy { get; set; }

    public DateTime JoinedAt { get; set; }
}
