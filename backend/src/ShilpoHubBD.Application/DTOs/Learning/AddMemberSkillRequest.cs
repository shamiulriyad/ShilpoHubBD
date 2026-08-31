using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.Learning;

public class AddMemberSkillRequest
{
    public Guid HeritageSkillId { get; set; }
    public SkillLevel Level { get; set; } = SkillLevel.Beginner;
}
