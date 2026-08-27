using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Innovation;

public class SubmissionTeamMember
{
    public Guid Id { get; set; }

    public Guid HeritageInnovationSubmissionId { get; set; }
    public HeritageInnovationSubmission Submission { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? RoleOnTeam { get; set; }

    public Guid AddedByUserId { get; set; }
    public User AddedBy { get; set; } = null!;

    public DateTime AddedAt { get; set; }
}
