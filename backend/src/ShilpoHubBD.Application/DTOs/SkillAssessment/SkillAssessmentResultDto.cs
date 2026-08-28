namespace ShilpoHubBD.Application.DTOs.SkillAssessment;

public class SkillAssessmentResultDto
{
    public Guid Id { get; set; }
    public Guid HeritageSkillId { get; set; }
    public string HeritageSkillName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public List<RecommendedSkillDto> RecommendedSkills { get; set; } = new();
    public DateTime AssessedAt { get; set; }
}
