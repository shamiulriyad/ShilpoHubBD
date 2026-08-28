using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.Employment;

public class AddJobSkillRequirementRequest
{
    public Guid HeritageSkillId { get; set; }
    public SkillLevel MinLevel { get; set; } = SkillLevel.Beginner;
    public bool IsRequired { get; set; } = true;
}
