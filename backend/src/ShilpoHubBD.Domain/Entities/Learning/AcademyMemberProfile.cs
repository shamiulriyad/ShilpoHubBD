using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Learning;

public class AcademyMemberProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public AcademyMemberRole Role { get; set; } = AcademyMemberRole.Learner;
    public string Bio { get; set; } = string.Empty;
    public string LearningPreferences { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<AcademyMemberSkill> Skills { get; set; } = new List<AcademyMemberSkill>();
}
