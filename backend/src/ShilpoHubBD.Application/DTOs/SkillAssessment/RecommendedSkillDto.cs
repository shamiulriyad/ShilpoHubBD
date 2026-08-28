namespace ShilpoHubBD.Application.DTOs.SkillAssessment;

public class RecommendedSkillDto
{
    public Guid HeritageSkillId { get; set; }
    public string HeritageSkillName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
