namespace ShilpoHubBD.Application.DTOs.Employment;

public class JobSkillRequirementDto
{
    public Guid Id { get; set; }
    public Guid JobListingId { get; set; }
    public Guid HeritageSkillId { get; set; }
    public string HeritageSkillName { get; set; } = string.Empty;
    public string MinLevel { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}
