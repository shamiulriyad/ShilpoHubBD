using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Mentorship;

public class MentorshipRequest
{
    public Guid Id { get; set; }

    public Guid MentorProfileId { get; set; }
    public MentorProfile MentorProfile { get; set; } = null!;

    public Guid LearnerUserId { get; set; }
    public User Learner { get; set; } = null!;

    public Guid? HeritageSkillId { get; set; }
    public HeritageSkill? HeritageSkill { get; set; }

    public string Message { get; set; } = string.Empty;

    public MentorshipRequestStatus Status { get; set; } = MentorshipRequestStatus.Pending;

    public DateTime RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
}
