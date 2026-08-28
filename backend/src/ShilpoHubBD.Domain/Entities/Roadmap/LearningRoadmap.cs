using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Roadmap;

public class LearningRoadmap
{
    public Guid Id { get; set; }

    public Guid AcademyMemberProfileId { get; set; }
    public AcademyMemberProfile AcademyMemberProfile { get; set; } = null!;

    public string Goal { get; set; } = string.Empty;

    public Guid? TargetHeritageSkillId { get; set; }
    public HeritageSkill? TargetHeritageSkill { get; set; }

    public RoadmapStatus Status { get; set; } = RoadmapStatus.Active;

    public DateTime GeneratedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<RoadmapMilestone> Milestones { get; set; } = new List<RoadmapMilestone>();
}
