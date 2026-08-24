using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.SkillAssessment;

public class SkillAssessmentProviderResult
{
    public SkillLevel Level { get; set; }
    public decimal Score { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<RecommendedSkillResult> RecommendedSkills { get; set; } = new();
}
