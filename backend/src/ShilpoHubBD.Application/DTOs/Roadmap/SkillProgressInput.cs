using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class SkillProgressInput
{
    public Guid HeritageSkillId { get; set; }
    public string Name { get; set; } = string.Empty;
    public SkillLevel? CurrentLevel { get; set; }
}
