namespace ShilpoHubBD.Application.DTOs.Learning;

public class AcademyMemberSkillDto
{
    public Guid Id { get; set; }
    public Guid HeritageSkillId { get; set; }
    public string HeritageSkillName { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
}
