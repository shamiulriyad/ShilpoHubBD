using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Employment;

public class JobSkillRequirement
{
    public Guid Id { get; set; }

    public Guid JobListingId { get; set; }
    public JobListing JobListing { get; set; } = null!;

    public Guid HeritageSkillId { get; set; }
    public HeritageSkill HeritageSkill { get; set; } = null!;

    public SkillLevel MinLevel { get; set; } = SkillLevel.Beginner;
    public bool IsRequired { get; set; } = true;
}
