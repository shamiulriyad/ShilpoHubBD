namespace ShilpoHubBD.Application.DTOs.Learning;

public class AcademyMemberProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string LearningPreferences { get; set; } = string.Empty;
    public List<AcademyMemberSkillDto> Skills { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
