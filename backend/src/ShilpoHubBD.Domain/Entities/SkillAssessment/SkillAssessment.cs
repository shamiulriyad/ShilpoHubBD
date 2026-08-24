using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.SkillAssessment;

public class SkillAssessment
{
    public Guid Id { get; set; }

    public Guid AcademyMemberProfileId { get; set; }
    public AcademyMemberProfile AcademyMemberProfile { get; set; } = null!;

    public Guid HeritageSkillId { get; set; }
    public HeritageSkill HeritageSkill { get; set; } = null!;

    public SkillLevel Level { get; set; }
    public decimal Score { get; set; }
    public string Summary { get; set; } = string.Empty;

    public DateTime AssessedAt { get; set; }

    public ICollection<SkillAssessmentInsight> Insights { get; set; } = new List<SkillAssessmentInsight>();
    public ICollection<SkillAssessmentRecommendedSkill> RecommendedSkills { get; set; } = new List<SkillAssessmentRecommendedSkill>();
}
