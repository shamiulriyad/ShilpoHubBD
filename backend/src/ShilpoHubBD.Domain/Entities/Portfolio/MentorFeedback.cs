using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Portfolio;

public class MentorFeedback
{
    public Guid Id { get; set; }

    public Guid MentorProfileId { get; set; }
    public MentorProfile MentorProfile { get; set; } = null!;

    public Guid LearnerUserId { get; set; }
    public User Learner { get; set; } = null!;

    public Guid? HeritageSkillId { get; set; }
    public HeritageSkill? HeritageSkill { get; set; }

    public string Message { get; set; } = string.Empty;
    public int? Rating { get; set; }

    public DateTime CreatedAt { get; set; }
}
