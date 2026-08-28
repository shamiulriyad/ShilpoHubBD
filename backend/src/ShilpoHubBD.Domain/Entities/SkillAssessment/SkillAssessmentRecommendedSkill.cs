using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.SkillAssessment;

public class SkillAssessmentRecommendedSkill
{
    public Guid Id { get; set; }

    public Guid SkillAssessmentId { get; set; }
    public SkillAssessment SkillAssessment { get; set; } = null!;

    public Guid HeritageSkillId { get; set; }
    public HeritageSkill HeritageSkill { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
