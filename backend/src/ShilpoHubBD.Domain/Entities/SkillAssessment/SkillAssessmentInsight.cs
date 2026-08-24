namespace ShilpoHubBD.Domain.Entities.SkillAssessment;

public class SkillAssessmentInsight
{
    public Guid Id { get; set; }

    public Guid SkillAssessmentId { get; set; }
    public SkillAssessment SkillAssessment { get; set; } = null!;

    public InsightType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
