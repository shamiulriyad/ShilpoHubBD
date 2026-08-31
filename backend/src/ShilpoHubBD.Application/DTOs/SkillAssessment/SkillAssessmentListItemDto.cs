namespace ShilpoHubBD.Application.DTOs.SkillAssessment;

public class SkillAssessmentListItemDto
{
    public Guid Id { get; set; }
    public Guid HeritageSkillId { get; set; }
    public string HeritageSkillName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public DateTime AssessedAt { get; set; }
}
